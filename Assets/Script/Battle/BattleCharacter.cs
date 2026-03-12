using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BattleCharacter
{
    public CharacterData OriginData { get; private set; }
    public int CurrentHealth { get; private set; }
    public int CurrentSanity { get; private set; }
    public int CurrentSpeed { get; private set; }
    public SkillDeckSystem DeckSystem { get; private set; }

    // [신규 추가] UI 즉각 갱신 요청 이벤트
    public event System.Action OnUIUpdateRequest;

    // -----------------------------
    // 흐트러짐 시스템
    // -----------------------------
    public int StaggerLevel { get; private set; } = 0;
    private int _brokenThresholdsCount = 0;
    public int[] RuntimeStaggerThresholds { get; private set; }

    // -----------------------------
    // 상태이상
    // -----------------------------
    public List<StatusEffect> ActiveEffects { get; private set; }

    // 전투 리스너
    private List<IBattleListener> _battleListeners = new List<IBattleListener>();

    public BattleCharacter(CharacterData data)
    {
        OriginData = data;
        CurrentHealth = data.maxHealth;
        CurrentSanity = 0;

        DeckSystem = new SkillDeckSystem();
        DeckSystem.Initialize(data.skill1, data.skill2, data.skill3);

        _battleListeners = new List<IBattleListener>();
        ActiveEffects = new List<StatusEffect>();

        if (data.staggerThresholds != null)
        {
            RuntimeStaggerThresholds = (int[])data.staggerThresholds.Clone();
        }
        else
        {
            RuntimeStaggerThresholds = new int[0];
        }

        RollSpeed();
    }

    // -----------------------------
    // 상태이상 추가
    // -----------------------------
    public void AddStatusEffect(StatusEffectType type, int potency, int count)
    {
        StatusEffect existingEffect = ActiveEffects.Find(e => e.effectType == type);

        if (existingEffect != null)
        {
            // 횟수가 0인 상태에서 위력이 추가되면 횟수 1 자동 부여
            if (existingEffect.count <= 0 && potency > 0 && count == 0)
                count = 1;
            // 위력이 0인 상태에서 횟수가 추가되면 위력 1 자동 부여
            if (existingEffect.potency <= 0 && count > 0 && potency == 0)
                potency = 1;
            existingEffect.Add(potency, count);
        }
        else
        {
            // 새로 부여할 때 횟수 없이 위력만 들어오면 횟수 1 자동 부여
            if (potency > 0 && count == 0)
                count = 1;
            // 새로 부여할 때 위력 없이 횟수만 들어오면 위력 1 자동 부여
            if (count > 0 && potency == 0)
                potency = 1;
            existingEffect = new StatusEffect(type, potency, count);
            ActiveEffects.Add(existingEffect);
        }

        if (type == StatusEffectType.Charge && existingEffect.count > 20)
        {
            existingEffect.count = 20;
        }

        Debug.Log($"[System] {OriginData.characterName}에게 {type} 부여됨. (위력: {existingEffect.potency}, 횟수: {existingEffect.count})");
        OnUIUpdateRequest?.Invoke();
    }

    // -----------------------------
    // 턴 종료 시 상태이상 정산
    // -----------------------------
    public void ProcessTurnEndStatusEffects()
    {
        for (int i = ActiveEffects.Count - 1; i >= 0; i--)
        {
            var effect = ActiveEffects[i];

            if (effect.count <= 0)
            {
                ActiveEffects.RemoveAt(i);
                continue;
            }

            switch (effect.effectType)
            {
                case StatusEffectType.Burn:
                    TakeDamage(effect.potency);
                    Debug.Log($"[StatusEffect] 화상 발동! {OriginData.characterName}가 {effect.potency} 고정 피해를 입음.");
                    effect.count--;
                    break;
                case StatusEffectType.Tremor:
                case StatusEffectType.Poise:
                case StatusEffectType.Charge:
                    effect.count--;
                    break;
            }

            if (effect.count <= 0)
            {
                ActiveEffects.RemoveAt(i);
            }
        }
    }

    // -----------------------------
    // 리스너 시스템
    // -----------------------------
    public void AddListener(IBattleListener listener)
    {
        _battleListeners.Add(listener);
        _battleListeners = _battleListeners.OrderBy(l => l.Priority).ToList();
    }

    public void RemoveListener(IBattleListener listener)
    {
        _battleListeners.Remove(listener);
    }

    public void TriggerHook(System.Action<IBattleListener> hookAction)
    {
        var listenersSnapshot = _battleListeners.ToList();
        foreach (var listener in listenersSnapshot)
        {
            hookAction.Invoke(listener);
        }
    }

    // -----------------------------
    // 속도 굴림
    // -----------------------------
    public void RollSpeed()
    {
        CurrentSpeed = Random.Range(OriginData.minSpeed, OriginData.maxSpeed + 1);
    }

    // -----------------------------
    // 정신력 변경
    // -----------------------------
    public void ChangeSanity(int amount)
    {
        CurrentSanity += amount;
        CurrentSanity = Mathf.Clamp(CurrentSanity, OriginData.minSanity, OriginData.maxSanity);

        Debug.Log($"[System] {OriginData.characterName}의 정신력이 {CurrentSanity}가 되었습니다.");

        // [신규 추가] 정신력 수치가 변경된 직후 UI 갱신 이벤트 호출
        OnUIUpdateRequest?.Invoke();
    }

    // -----------------------------
    // 데미지 처리 및 흐트러짐 검사
    // -----------------------------
    public void TakeDamage(int damage, BattleCharacter attacker = null)
    {
        if (CurrentHealth <= 0) return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
        Debug.Log($"[System] {OriginData.characterName}가 {damage} 피해를 입음 (HP:{CurrentHealth})");

        // [신규 추가] 체력 수치가 변경된 직후 UI 갱신 이벤트 호출
        OnUIUpdateRequest?.Invoke();

        CheckStaggerThresholds();

        if (CurrentHealth == 0)
        {
            Debug.Log($"[System] {OriginData.characterName} 사망!");
            if (BattleEventManager.Instance != null)
            {
                BattleEventManager.Instance.TriggerCharacterDeath(this, attacker);
            }
        }
    }

    // -----------------------------
    // 흐트러짐 선 연산 로직
    // -----------------------------
    public void AddStagger(int amount)
    {
        if (RuntimeStaggerThresholds == null || RuntimeStaggerThresholds.Length == 0) return;

        for (int i = _brokenThresholdsCount; i < RuntimeStaggerThresholds.Length; i++)
        {
            RuntimeStaggerThresholds[i] += amount;
            RuntimeStaggerThresholds[i] = Mathf.Min(RuntimeStaggerThresholds[i], OriginData.maxHealth);
        }

        Debug.Log($"[System] {OriginData.characterName}의 흐트러짐 선이 {amount}만큼 전진했습니다.");
        CheckStaggerThresholds();
    }

    private void CheckStaggerThresholds()
    {
        if (RuntimeStaggerThresholds == null || RuntimeStaggerThresholds.Length == 0) return;

        for (int i = _brokenThresholdsCount; i < RuntimeStaggerThresholds.Length; i++)
        {
            int thresholdHp = RuntimeStaggerThresholds[i];
            if (CurrentHealth <= thresholdHp)
            {
                _brokenThresholdsCount++;
                StaggerLevel++;
                Debug.Log($"[System] {OriginData.characterName} 흐트러짐 선 파괴! (남은 체력: {CurrentHealth} <= 기준치: {thresholdHp}) 단계 상승 → {StaggerLevel}");
            }
            else
            {
                break;
            }
        }
    }

    // -----------------------------
    // 턴 시작시 흐트러짐 회복
    // -----------------------------
    public void RecoverStagger()
    {
        if (StaggerLevel > 0)
        {
            StaggerLevel--;
            Debug.Log($"[System] {OriginData.characterName} 흐트러짐 단계 감소 → {StaggerLevel}");
        }
    }

    // -----------------------------
    // 캐릭터 초기화
    // -----------------------------
    public void ResetCharacter()
    {
        CurrentHealth = OriginData.maxHealth;
        CurrentSanity = 0;

        ActiveEffects.Clear();

        DeckSystem.Initialize(OriginData.skill1, OriginData.skill2, OriginData.skill3);

        RollSpeed();

        StaggerLevel = 0;
        _brokenThresholdsCount = 0;

        if (OriginData.staggerThresholds != null)
        {
            RuntimeStaggerThresholds = (int[])OriginData.staggerThresholds.Clone();
        }

        Debug.Log($"[System] {OriginData.characterName}의 상태가 초기화되었습니다.");

        // [신규 추가] 상태가 초기화되었으므로 화면 동기화를 위해 이벤트 호출
        OnUIUpdateRequest?.Invoke();
    }
}