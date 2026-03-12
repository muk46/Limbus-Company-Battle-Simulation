using System.Collections.Generic;
using UnityEngine;

// 공격 명령 한 줄을 담을 데이터 구조체
public class BattleAction
{
    public BattleCharacter Attacker;
    public RuntimeSkill UsedSkill;
    public BattleCharacter Target;
}

public class BattleManager : MonoBehaviour
{
    // 어디서든 UI가 BattleManager를 찾을 수 있도록 싱글톤 인스턴스화
    public static BattleManager Instance { get; private set; }

    public BattleEvents Events { get; private set; } = new BattleEvents();

    [Header("Debug")]
    [SerializeField] private CoinForceMode coinForceMode = CoinForceMode.Random;
    [Tooltip("Custom 모드 시 코인 패턴 (true=앞, false=뒤). 예: 앞뒤앞 → ✓✗✓")]
    [SerializeField] private bool[] customCoinPattern;

    [Header("Team Data Settings")]
    public List<CharacterData> playerTeamData;
    public List<CharacterData> enemyTeamData;

    [Header("World Spawning")]
    public GameObject characterEntityPrefab;
    public Transform[] playerSpawnPoints;
    public Transform[] enemySpawnPoints;

    // 1. 순수 논리 데이터 리스트 (HP, 덱 관리용)
    public List<BattleCharacter> PlayerTeam { get; private set; } = new List<BattleCharacter>();
    public List<BattleCharacter> EnemyTeam { get; private set; } = new List<BattleCharacter>();

    // 2. 시각적 월드 엔티티 리스트 (애니메이션, 위치 이동 제어용)
    public List<CharacterEntityController> PlayerEntities { get; private set; } = new List<CharacterEntityController>();
    public List<CharacterEntityController> EnemyEntities { get; private set; } = new List<CharacterEntityController>();

    // 추가: 플레이어가 UI로 드래그한 공격 명령들이 쌓이는 대기열
    public List<BattleAction> ActionQueue { get; private set; } = new List<BattleAction>();

    private IBattleState _currentState;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // 아군 데이터 인스턴스화 및 월드 스폰
        for (int i = 0; i < playerTeamData.Count; i++)
        {
            if (playerTeamData[i] != null)
            {
                BattleCharacter newPlayer = new BattleCharacter(playerTeamData[i]);
                PlayerTeam.Add(newPlayer);

                if (playerSpawnPoints != null && i < playerSpawnPoints.Length && playerSpawnPoints[i] != null)
                {
                    // 프리팹 생성 후 CharacterEntityController 컴포넌트 추출
                    GameObject obj = Instantiate(characterEntityPrefab, playerSpawnPoints[i].position, Quaternion.identity);
                    CharacterEntityController entity = obj.GetComponent<CharacterEntityController>();

                    if (entity != null)
                    {
                        // 데이터 주입 및 리스트 추가
                        entity.Initialize(newPlayer);
                        PlayerEntities.Add(entity);
                    }
                }
            }
        }

        // 적군 데이터 인스턴스화 및 월드 스폰
        for (int i = 0; i < enemyTeamData.Count; i++)
        {
            if (enemyTeamData[i] != null)
            {
                BattleCharacter newEnemy = new BattleCharacter(enemyTeamData[i]);
                EnemyTeam.Add(newEnemy);

                if (enemySpawnPoints != null && i < enemySpawnPoints.Length && enemySpawnPoints[i] != null)
                {
                    GameObject obj = Instantiate(characterEntityPrefab, enemySpawnPoints[i].position, Quaternion.identity);
                    CharacterEntityController entity = obj.GetComponent<CharacterEntityController>();

                    if (entity != null)
                    {
                        entity.Initialize(newEnemy);
                        EnemyEntities.Add(entity);
                    }
                }
            }
        }

        Events.OnBattleInit?.Invoke(PlayerTeam, EnemyTeam);

        // [추가된 로직] 전역 이벤트 매니저에 전투 시작 알림 (기프트 시스템 연동용)
        if (BattleEventManager.Instance != null)
        {
            BattleEventManager.Instance.TriggerBattleStart();
        }

        ChangeState(new BattleWaitState(this));
    }

    // 타겟팅이 완료되었을 때 UI에서 호출하는 등록 함수
    public void RegisterAction(BattleCharacter attacker, RuntimeSkill skill, BattleCharacter target)
    {
        if (ActionQueue == null) ActionQueue = new List<BattleAction>();

        // 1. 기존에 해당 캐릭터가 등록했던 행동이 있다면 모두 삭제 (취소 로직)
        int removedCount = ActionQueue.RemoveAll(action => action.Attacker == attacker);

        if (removedCount > 0)
        {
            Debug.Log($"[System] {attacker.OriginData.characterName}의 이전 행동이 취소되고 새로운 스킬로 교체됩니다.");
        }

        // 2. 새로운 행동 생성 및 등록
        BattleAction newAction = new BattleAction
        {
            Attacker = attacker,
            UsedSkill = skill,
            Target = target
        };

        ActionQueue.Add(newAction);

        Debug.Log($"[System] 행동 등록 완료: {attacker.OriginData.characterName} -> {target.OriginData.characterName} (스킬: {skill.OriginData.skillName})");
    }

    public void ChangeState(IBattleState newState)
    {
        _currentState?.Exit();
        _currentState = newState;
        _currentState?.Enter();
    }

    private void Update()
    {
        ClashCalculator.ForceCoinMode = coinForceMode;
        ClashCalculator.CustomCoinPattern = customCoinPattern;
        _currentState?.Execute();
    }

    public Coroutine RunRoutine(System.Collections.IEnumerator routine) => StartCoroutine(routine);
    public void StopRoutine(Coroutine routine) => StopCoroutine(routine);
    public void SwapCharacter(bool isPlayerTeam, int index, CharacterData newData)
    {
        if (newData == null) return;

        List<BattleCharacter> targetTeam = isPlayerTeam ? PlayerTeam : EnemyTeam;
        List<CharacterEntityController> targetEntities = isPlayerTeam ? PlayerEntities : EnemyEntities;
        Transform[] spawnPoints = isPlayerTeam ? playerSpawnPoints : enemySpawnPoints;

        // 인덱스 범위 검사
        if (index < 0 || index >= targetTeam.Count)
        {
            Debug.LogWarning("[System] 교체하려는 캐릭터의 인덱스가 범위를 벗어났습니다.");
            return;
        }

        // 1. 기존 엔티티 파괴
        if (targetEntities[index] != null)
        {
            Destroy(targetEntities[index].gameObject);
        }

        // 2. 새로운 논리 데이터 생성 및 교체
        BattleCharacter newCharacter = new BattleCharacter(newData);
        targetTeam[index] = newCharacter;

        // 3. 새로운 시각적 엔티티 스폰 및 초기화
        if (spawnPoints != null && index < spawnPoints.Length && spawnPoints[index] != null)
        {
            GameObject obj = Instantiate(characterEntityPrefab, spawnPoints[index].position, Quaternion.identity);
            CharacterEntityController entity = obj.GetComponent<CharacterEntityController>();

            if (entity != null)
            {
                entity.Initialize(newCharacter);
                targetEntities[index] = entity;
            }
        }

        Debug.Log($"[System] {index}번 슬롯의 캐릭터가 {newData.characterName}(으)로 교체되었습니다.");

        // 4. UI 갱신을 위한 이벤트 발생
        Events.OnCharacterSwapped?.Invoke(index, isPlayerTeam, newCharacter);
    }

    // 훈련장 초기화 로직 (체력 원상복구 및 대기 상태로 회귀)
    public void ResetBattle()
    {
        // 등록된 행동 대기열 취소
        ActionQueue.Clear();

        // 양 진영 모든 캐릭터 상태 초기화
        foreach (var chara in PlayerTeam) chara.ResetCharacter();
        foreach (var chara in EnemyTeam) chara.ResetCharacter();

        // 진영별 UI 및 월드 엔티티 갱신
        foreach (var entity in PlayerEntities) entity.UpdateUI();
        foreach (var entity in EnemyEntities) entity.UpdateUI();

        Debug.Log("[System] 전투 상태가 초기화되었습니다.");

        // UI 전역 갱신 이벤트 발생
        Events.OnBattleReset?.Invoke();

        // 다시 스킬을 선택할 수 있는 Wait 상태로 강제 전환
        ChangeState(new BattleWaitState(this));
    }
    [Header("Training Room Settings")]
    public bool isEnemyAggressive = false; // 기본값은 샌드백(비공격) 모드

    // 적군 행동 자동 생성 로직
    public void GenerateEnemyActions()
    {
        // 공격 스위치가 꺼져있으면 모든 선을 숨기고 종료
        if (!isEnemyAggressive)
        {
            foreach (var entity in EnemyEntities) entity.HideTargetingLine();
            return;
        }

        // 새 턴의 행동을 정하기 전, 큐에 남아있는 적의 예전 행동을 청소
        ActionQueue.RemoveAll(a => EnemyTeam.Contains(a.Attacker));

        for (int i = 0; i < EnemyTeam.Count; i++)
        {
            var enemy = EnemyTeam[i];
            if (enemy.CurrentHealth <= 0) continue;

            var validPlayers = PlayerTeam.FindAll(p => p.CurrentHealth > 0);
            if (validPlayers.Count == 0) continue;
            var target = validPlayers[UnityEngine.Random.Range(0, validPlayers.Count)];

            RuntimeSkill skillToUse = enemy.DeckSystem.ConsumeSkill();

            if (skillToUse != null)
            {
                ActionQueue.Add(new BattleAction
                {
                    Attacker = enemy,
                    UsedSkill = skillToUse,
                    Target = target
                });

                // [추가된 로직] 타겟을 향해 붉은색 지정 선(Arrow) 출력
                int targetIndex = PlayerTeam.IndexOf(target);
                if (targetIndex >= 0 && EnemyEntities[i] != null && PlayerEntities[targetIndex] != null)
                {
                    EnemyEntities[i].ShowTargetingLine(PlayerEntities[targetIndex].transform);
                }
            }
        }
    }
}
