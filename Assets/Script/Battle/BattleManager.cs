using System.Collections.Generic;
using UnityEngine;

public class BattleAction
{
    public BattleCharacter Attacker;
    public RuntimeSkill    UsedSkill;
    public BattleCharacter Target;
}

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [Header("Database")]
    [Tooltip("인스펙터에서 StatusEffectDatabase 에셋을 할당하세요.")]
    public StatusEffectDatabase statusEffectDatabase;

    [Header("Team Data Settings")]
    public List<CharacterData> playerTeamData;
    public List<CharacterData> enemyTeamData;

    [Header("World Spawning")]
    public GameObject  characterEntityPrefab;
    public Transform[] playerSpawnPoints;
    public Transform[] enemySpawnPoints;

    [Header("Training Room Settings")]
    public bool isEnemyAggressive = false;

    public List<BattleCharacter>        PlayerTeam     { get; private set; } = new List<BattleCharacter>();
    public List<BattleCharacter>        EnemyTeam      { get; private set; } = new List<BattleCharacter>();
    public List<CharacterEntityController> PlayerEntities { get; private set; } = new List<CharacterEntityController>();
    public List<CharacterEntityController> EnemyEntities  { get; private set; } = new List<CharacterEntityController>();
    public List<BattleAction>           ActionQueue    { get; private set; } = new List<BattleAction>();

    private IBattleState _currentState;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        // 이벤트 버스 초기화 (이전 씬 구독 잔재 제거)
        BattleEventBus.ClearAll();

        // 상태이상 데이터베이스 초기화
        if (statusEffectDatabase != null)
            statusEffectDatabase.Initialize();
        else
            Debug.LogWarning("[BattleManager] StatusEffectDatabase가 할당되지 않았습니다.");
    }

    private void Start()
    {
        SpawnTeam(playerTeamData, PlayerTeam, PlayerEntities, playerSpawnPoints);
        SpawnTeam(enemyTeamData,  EnemyTeam,  EnemyEntities,  enemySpawnPoints);

        BattleEventBus.Raise_OnBattleInit(PlayerTeam, EnemyTeam);
        BattleEventBus.Raise_OnBattleStart();

        ChangeState(new BattleWaitState(this));
    }

    private void SpawnTeam(
        List<CharacterData> dataList,
        List<BattleCharacter> teamList,
        List<CharacterEntityController> entityList,
        Transform[] spawnPoints)
    {
        for (int i = 0; i < dataList.Count; i++)
        {
            if (dataList[i] == null) continue;

            var character = new BattleCharacter(dataList[i]);
            teamList.Add(character);

            if (spawnPoints != null && i < spawnPoints.Length && spawnPoints[i] != null)
            {
                var obj    = Instantiate(characterEntityPrefab, spawnPoints[i].position, Quaternion.identity);
                var entity = obj.GetComponent<CharacterEntityController>();
                if (entity != null)
                {
                    entity.Initialize(character);
                    entityList.Add(entity);
                }
            }
        }
    }

    public void RegisterAction(BattleCharacter attacker, RuntimeSkill skill, BattleCharacter target)
    {
        if (ActionQueue == null) ActionQueue = new List<BattleAction>();

        int removed = ActionQueue.RemoveAll(a => a.Attacker == attacker);
        if (removed > 0)
            Debug.Log($"[System] {attacker.OriginData.characterName}의 이전 행동이 교체되었습니다.");

        ActionQueue.Add(new BattleAction
        {
            Attacker  = attacker,
            UsedSkill = skill,
            Target    = target
        });

        Debug.Log($"[System] 행동 등록: {attacker.OriginData.characterName} → {target.OriginData.characterName} (스킬: {skill.OriginData.skillName})");
    }

    public void ChangeState(IBattleState newState)
    {
        _currentState?.Exit();
        _currentState = newState;
        _currentState?.Enter();
    }

    private void Update()
    {
        _currentState?.Execute();
    }

    public Coroutine RunRoutine(System.Collections.IEnumerator routine) => StartCoroutine(routine);
    public void StopRoutine(Coroutine routine) => StopCoroutine(routine);

    public void SwapCharacter(bool isPlayerTeam, int index, CharacterData newData)
    {
        if (newData == null) return;

        var teamList   = isPlayerTeam ? PlayerTeam     : EnemyTeam;
        var entityList = isPlayerTeam ? PlayerEntities : EnemyEntities;
        var spawnPoints = isPlayerTeam ? playerSpawnPoints : enemySpawnPoints;

        if (index < 0 || index >= teamList.Count)
        {
            Debug.LogWarning("[System] 교체하려는 캐릭터의 인덱스가 범위를 벗어났습니다.");
            return;
        }

        if (entityList[index] != null) Destroy(entityList[index].gameObject);

        var newCharacter = new BattleCharacter(newData);
        teamList[index] = newCharacter;

        if (spawnPoints != null && index < spawnPoints.Length && spawnPoints[index] != null)
        {
            var obj    = Instantiate(characterEntityPrefab, spawnPoints[index].position, Quaternion.identity);
            var entity = obj.GetComponent<CharacterEntityController>();
            if (entity != null)
            {
                entity.Initialize(newCharacter);
                entityList[index] = entity;
            }
        }

        Debug.Log($"[System] {index}번 슬롯의 캐릭터가 {newData.characterName}(으)로 교체되었습니다.");
        BattleEventBus.Raise_OnCharacterSwapped(index, isPlayerTeam, newCharacter);
    }

    public void ResetBattle()
    {
        ActionQueue.Clear();

        foreach (var c in PlayerTeam) c.ResetCharacter();
        foreach (var c in EnemyTeam)  c.ResetCharacter();

        foreach (var e in PlayerEntities) e.UpdateUI();
        foreach (var e in EnemyEntities)  e.UpdateUI();

        Debug.Log("[System] 배틀 상태가 초기화되었습니다.");
        BattleEventBus.Raise_OnBattleReset();

        ChangeState(new BattleWaitState(this));
    }

    public void GenerateEnemyActions()
    {
        ActionQueue.RemoveAll(a => EnemyTeam.Contains(a.Attacker));

        if (!isEnemyAggressive)
        {
            foreach (var entity in EnemyEntities) entity.HideTargetingLine();
            return;
        }

        for (int i = 0; i < EnemyTeam.Count; i++)
        {
            var enemy = EnemyTeam[i];
            if (enemy.CurrentHealth <= 0) continue;

            var validPlayers = PlayerTeam.FindAll(p => p.CurrentHealth > 0);
            if (validPlayers.Count == 0) continue;

            var target = validPlayers[Random.Range(0, validPlayers.Count)];
            var skills = enemy.DeckSystem.GetVisibleSkills(1);
            if (skills.Length == 0 || skills[0] == null) continue;

            ActionQueue.Add(new BattleAction
            {
                Attacker  = enemy,
                UsedSkill = skills[0],
                Target    = target
            });

            int targetIndex = PlayerTeam.IndexOf(target);
            if (targetIndex >= 0 && EnemyEntities[i] != null && PlayerEntities[targetIndex] != null)
                EnemyEntities[i].ShowTargetingLine(PlayerEntities[targetIndex].transform);
        }
    }
}
