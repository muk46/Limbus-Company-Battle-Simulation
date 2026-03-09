using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BattleCharacter
{
    public CharacterData OriginData { get; private set; }
    public int CurrentHealth  { get; private set; }
    public int CurrentSanity  { get; private set; }
    public int CurrentSpeed   { get; private set; }
    public SkillDeckSystem DeckSystem { get; private set; }

    // ── 스태거 ───────────────────────────────────────────
    public int StaggerLevel     { get; private set; } = 0;
    public int StaggerGauge     { get; private set; } = 0;
    public int StaggerThreshold { get; private set; } = 100;

    // ── 상태이상 ─────────────────────────────────────────
    public List<StatusEffect> ActiveEffects { get; private set; }

    // ── 리스너 훅 ────────────────────────────────────────
    private List<IBattleListener> _battleListeners = new List<IBattleListener>();

    public BattleCharacter(CharacterData data)
    {
        OriginData    = data;
        CurrentHealth = data.maxHealth;
        CurrentSanity = 0;

        DeckSystem = new SkillDeckSystem();
        DeckSystem.Initialize(data.skill1, data.skill2, data.skill3);

        _battleListeners = new List<IBattleListener>();
        ActiveEffects    = new List<StatusEffect>();

        RollSpeed();
    }

    // ── 상태이상 ─────────────────────────────────────────

    public void AddStatusEffect(StatusEffectType type, int potency, int count)
    {
        StatusEffect existing = ActiveEffects.Find(e => e.effectType == type);

        if (existing != null)
        {
            existing.Add(potency, count);
        }
        else
        {
            existing = new StatusEffect(type, potency, count);
            ActiveEffects.Add(existing);
        }

        if (type == StatusEffectType.Charge && existing.count > 20)
            existing.count = 20;

        Debug.Log($"[System] {OriginData.characterName}에게 {type} 부여. (강도: {existing.potency}, 횟수: {existing.count})");
    }

    /// <summary>
    /// 턴 종료 시 상태이상 처리.
    /// 각 상태이상의 동작은 StatusEffectDatabase에 등록된 StatusEffectData가 정의합니다.
    /// </summary>
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

            StatusEffectData data = StatusEffectDatabase.Instance?.Get(effect.effectType);
            data?.OnTurnEnd(this, effect);

            if (effect.count <= 0)
                ActiveEffects.RemoveAt(i);
        }
    }

    // ── 리스너 훅 ────────────────────────────────────────

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
        var snapshot = _battleListeners.ToList();
        foreach (var listener in snapshot)
            hookAction.Invoke(listener);
    }

    // ── 속도 ─────────────────────────────────────────────

    public void RollSpeed()
    {
        CurrentSpeed = Random.Range(OriginData.minSpeed, OriginData.maxSpeed + 1);
    }

    // ── 정신력 ───────────────────────────────────────────

    public void ChangeSanity(int amount)
    {
        CurrentSanity += amount;
        CurrentSanity = Mathf.Clamp(CurrentSanity, OriginData.minSanity, OriginData.maxSanity);
        Debug.Log($"[System] {OriginData.characterName}의 정신력이 {CurrentSanity}이 되었습니다.");
    }

    // ── 스태거 ───────────────────────────────────────────

    public void AddStagger(int amount)
    {
        StaggerGauge += amount;

        if (StaggerGauge >= StaggerThreshold)
        {
            StaggerGauge = 0;
            StaggerLevel++;
            Debug.Log($"[System] {OriginData.characterName} 스태거 단계 상승 → {StaggerLevel}");
        }
    }

    public void RecoverStagger()
    {
        if (StaggerLevel > 0)
        {
            StaggerLevel--;
            Debug.Log($"[System] {OriginData.characterName} 스태거 단계 회복 → {StaggerLevel}");
        }
    }

    // ── 피해 처리 ────────────────────────────────────────

    public void TakeDamage(int damage, BattleCharacter attacker = null)
    {
        if (CurrentHealth <= 0) return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
        AddStagger(damage / 2);
        Debug.Log($"[System] {OriginData.characterName}이 {damage} 피해를 받음 (HP:{CurrentHealth})");

        if (CurrentHealth == 0)
        {
            Debug.Log($"[System] {OriginData.characterName} 사망!");
            BattleEventBus.Raise_OnCharacterDeath(this, attacker);
        }
    }

    // ── 초기화 ───────────────────────────────────────────

    public void ResetCharacter()
    {
        CurrentHealth = OriginData.maxHealth;
        CurrentSanity = 0;
        StaggerGauge  = 0;
        StaggerLevel  = 0;

        ActiveEffects.Clear();
        DeckSystem.Initialize(OriginData.skill1, OriginData.skill2, OriginData.skill3);
        RollSpeed();

        Debug.Log($"[System] {OriginData.characterName}의 상태가 초기화되었습니다.");
    }
}
