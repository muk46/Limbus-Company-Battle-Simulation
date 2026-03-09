using UnityEngine;

/// <summary>
/// 상태이상의 발동 타이밍과 동작을 정의하는 ScriptableObject 기반 베이스.
/// 새 상태이상 추가 시 이 클래스를 상속하고 CreateAssetMenu로 에셋을 만들면 됩니다.
/// ClashCalculator나 BattleCharacter를 수정할 필요가 없습니다.
/// </summary>
public abstract class StatusEffectData : ScriptableObject
{
    public StatusEffectType effectType;

    /// <summary>턴 종료 시 발동. (Burn, Tremor, Poise, Charge)</summary>
    public virtual void OnTurnEnd(BattleCharacter owner, StatusEffect effect) { }

    /// <summary>코인을 굴릴 때 발동. (Bleed)</summary>
    public virtual void OnCoinRoll(BattleCharacter attacker, StatusEffect effect) { }

    /// <summary>타격이 적중했을 때 발동. (Rupture, Sinking)</summary>
    public virtual void OnHit(BattleCharacter attacker, BattleCharacter target, StatusEffect effect) { }

    /// <summary>크리티컬 배율 반환. (Poise)</summary>
    public virtual float GetCritMultiplier(BattleCharacter attacker, StatusEffect effect) => 1f;
}

// ── 화상 (Burn) ───────────────────────────────────────────────
[CreateAssetMenu(menuName = "Combat/Status Effects/Burn")]
public class StatusEffectData_Burn : StatusEffectData
{
    public override void OnTurnEnd(BattleCharacter owner, StatusEffect effect)
    {
        owner.TakeDamage(effect.potency);
        effect.count--;
        Debug.Log($"[StatusEffect] 화상 발동! {owner.OriginData.characterName}에게 {effect.potency} 피해.");
    }
}

// ── 출혈 (Bleed) ──────────────────────────────────────────────
[CreateAssetMenu(menuName = "Combat/Status Effects/Bleed")]
public class StatusEffectData_Bleed : StatusEffectData
{
    public override void OnCoinRoll(BattleCharacter attacker, StatusEffect effect)
    {
        if (effect.count <= 0) return;
        attacker.TakeDamage(effect.potency);
        effect.count--;
        Debug.Log($"[StatusEffect] 출혈 발동! {attacker.OriginData.characterName}에게 {effect.potency} 피해.");
    }
}

// ── 전율 (Tremor) ─────────────────────────────────────────────
[CreateAssetMenu(menuName = "Combat/Status Effects/Tremor")]
public class StatusEffectData_Tremor : StatusEffectData
{
    public override void OnTurnEnd(BattleCharacter owner, StatusEffect effect)
    {
        effect.count--;
    }
}

// ── 파열 (Rupture) ────────────────────────────────────────────
[CreateAssetMenu(menuName = "Combat/Status Effects/Rupture")]
public class StatusEffectData_Rupture : StatusEffectData
{
    public override void OnHit(BattleCharacter attacker, BattleCharacter target, StatusEffect effect)
    {
        if (effect.count <= 0) return;
        target.TakeDamage(effect.potency);
        effect.count--;
        Debug.Log($"[StatusEffect] 파열 발동! {target.OriginData.characterName}에게 {effect.potency} 피해.");
    }
}

// ── 침잠 (Sinking) ────────────────────────────────────────────
[CreateAssetMenu(menuName = "Combat/Status Effects/Sinking")]
public class StatusEffectData_Sinking : StatusEffectData
{
    public override void OnHit(BattleCharacter attacker, BattleCharacter target, StatusEffect effect)
    {
        if (effect.count <= 0) return;
        if (target.OriginData.maxSanity > 0)
            target.ChangeSanity(-effect.potency);
        else
            target.TakeDamage(Mathf.FloorToInt(effect.potency * target.OriginData.gloomRes));
        effect.count--;
    }
}

// ── 호기 (Poise) ──────────────────────────────────────────────
[CreateAssetMenu(menuName = "Combat/Status Effects/Poise")]
public class StatusEffectData_Poise : StatusEffectData
{
    public override float GetCritMultiplier(BattleCharacter attacker, StatusEffect effect)
    {
        if (effect.count <= 0) return 1f;
        if (Random.Range(0, 100) < (effect.potency * 5))
        {
            effect.count--;
            return 1.2f;
        }
        return 1f;
    }
}

// ── 충전 (Charge) ─────────────────────────────────────────────
[CreateAssetMenu(menuName = "Combat/Status Effects/Charge")]
public class StatusEffectData_Charge : StatusEffectData
{
    public override void OnTurnEnd(BattleCharacter owner, StatusEffect effect)
    {
        effect.count--;
    }
}
