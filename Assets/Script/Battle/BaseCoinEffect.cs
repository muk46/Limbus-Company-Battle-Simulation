using UnityEngine;
using System.Collections.Generic;

// [수정됨] ScriptableObject를 상속받도록 변경
public abstract class BaseCoinEffect : ScriptableObject
{
    [Header("발동 조건")]
    public EffectCondition triggerCondition;

    public abstract void Execute(BattleCharacter attacker, BattleCharacter target, RuntimeSkill skill);
}

// =========================================================
// 복합 조건 판별 클래스 (이 부분은 일반 클래스 유지)
// =========================================================
[System.Serializable]
public class CoinCondition
{
    [Header("조건 설정")]
    public ConditionTarget targetType;
    public ConditionCheckType checkType;
    [Tooltip("검사 타입이 상태이상일 때만 작동합니다.")]
    public StatusEffectType statusEffect;
    public CompareOperator operatorType;
    public int value;

    public bool Evaluate(BattleCharacter attacker, BattleCharacter target)
    {
        BattleCharacter subject = (targetType == ConditionTarget.Attacker) ? attacker : target;
        int currentValue = 0;

        switch (checkType)
        {
            case ConditionCheckType.StatusPotency:
                var eff1 = subject.ActiveEffects.Find(e => e.effectType == statusEffect);
                currentValue = eff1 != null ? eff1.potency : 0; break;
            case ConditionCheckType.StatusCount:
                var eff2 = subject.ActiveEffects.Find(e => e.effectType == statusEffect);
                currentValue = eff2 != null ? eff2.count : 0; break;
            case ConditionCheckType.Sanity: currentValue = subject.CurrentSanity; break;
            case ConditionCheckType.Speed: currentValue = subject.CurrentSpeed; break;
            case ConditionCheckType.SpeedDiff: currentValue = attacker.CurrentSpeed - target.CurrentSpeed; break;
        }

        switch (operatorType)
        {
            case CompareOperator.GreaterOrEqual: return currentValue >= value;
            case CompareOperator.LessOrEqual: return currentValue <= value;
            case CompareOperator.Equal: return currentValue == value;
            default: return false;
        }
    }
}

// =========================================================
// 이펙트 모듈 자식 클래스 모음 (각각 CreateAssetMenu 추가)
// =========================================================

[CreateAssetMenu(menuName = "Combat/Coin Effects/Apply Status")]
public class CoinEffect_ApplyStatus : BaseCoinEffect
{
    public StatusEffectType effectType;
    public int potency;
    public int count;

    public override void Execute(BattleCharacter attacker, BattleCharacter target, RuntimeSkill skill)
    {
        target.AddStatusEffect(effectType, potency, count);
    }
}

[CreateAssetMenu(menuName = "Combat/Coin Effects/Scale Status By Target")]
public class CoinEffect_ScaleStatusByTarget : BaseCoinEffect
{
    public StatusEffectType referenceEffect;
    public float divideBy = 3f;
    public StatusEffectType effectToApply;
    public int maxLimit = 5;

    public override void Execute(BattleCharacter attacker, BattleCharacter target, RuntimeSkill skill)
    {
        StatusEffect refEffect = target.ActiveEffects.Find(e => e.effectType == referenceEffect);
        int refPotency = refEffect != null ? refEffect.potency : 0;
        int calculatedPotency = Mathf.FloorToInt(refPotency / divideBy);
        calculatedPotency = Mathf.Min(calculatedPotency, maxLimit);

        if (calculatedPotency > 0)
        {
            target.AddStatusEffect(effectToApply, calculatedPotency, 0);
            Debug.Log($"[Effect] 비례 연산 발동: {target.OriginData.characterName}에게 {effectToApply} {calculatedPotency} 부여");
        }
    }
}

[CreateAssetMenu(menuName = "Combat/Coin Effects/Trigger Status")]
public class CoinEffect_TriggerStatus : BaseCoinEffect
{
    public StatusEffectType effectToTrigger;
    public int triggerTimes = 1;
    public int countToDecrease = 1; // 여기서는 보통 횟수 소모량을 정의

    public override void Execute(BattleCharacter attacker, BattleCharacter target, RuntimeSkill skill)
    {
        StatusEffect effect = target.ActiveEffects.Find(e => e.effectType == effectToTrigger);
        if (effect != null && effect.potency > 0 && effect.count > 0)
        {
            int actualTriggers = 0;
            // [수정] 루프 조건에 effect.count > 0을 직접 확인
            for (int i = 0; i < triggerTimes; i++)
            {
                if (effect.count > 0)
                {
                    target.TakeDamage(effect.potency);
                    effect.count--; // [핵심] 한 번 터질 때마다 횟수를 즉시 1 차감
                    actualTriggers++;
                }
                else
                {
                    break; // 횟수가 다 떨어지면 루프 중단
                }
            }
            // 최종적으로 추가 차감이 필요하다면 적용 (이미 다 깎았다면 0 유지)
            // effect.count -= (countToDecrease - 1); // 기획에 따라 조정

            Debug.Log($"[Effect] {effectToTrigger} {actualTriggers}회 연속 발동! 총 {effect.potency * actualTriggers} 피해. 남은 횟수: {Mathf.Max(0, effect.count)}");
        }
    }
}

[CreateAssetMenu(menuName = "Combat/Coin Effects/Tremor Burst")]
public class CoinEffect_TremorBurst : BaseCoinEffect
{
    public override void Execute(BattleCharacter attacker, BattleCharacter target, RuntimeSkill skill)
    {
        StatusEffect tremor = target.ActiveEffects.Find(e => e.effectType == StatusEffectType.Tremor);
        if (tremor != null && tremor.potency > 0 && tremor.count > 0)
        {
            target.AddStagger(tremor.potency);
            tremor.count -= 1;
            Debug.Log($"[Effect] 진동 폭발 발동! {target.OriginData.characterName}의 흐트러짐 게이지가 {tremor.potency} 증가했습니다.");
        }
    }
}

[CreateAssetMenu(menuName = "Combat/Coin Effects/Advanced Power Buff")]
public class CoinEffect_AdvancedPowerBuff : BaseCoinEffect
{
    [Header("발동 조건 리스트")]
    public List<CoinCondition> conditions = new List<CoinCondition>();
    [Header("위력 버프 설정")]
    public PowerModifierType targetPowerType;
    public int powerBonus = 1;

    public override void Execute(BattleCharacter attacker, BattleCharacter target, RuntimeSkill skill)
    {
        foreach (var cond in conditions)
        {
            if (!cond.Evaluate(attacker, target)) return;
        }

        switch (targetPowerType)
        {
            case PowerModifierType.BasePower: skill.CurrentBasePower += powerBonus; break;
            case PowerModifierType.CoinPower: skill.CurrentCoinPower += powerBonus; break;
            case PowerModifierType.ClashPower: skill.ClashPowerModifier += powerBonus; break;
            case PowerModifierType.FinalPower: skill.FinalPowerModifier += powerBonus; break;
        }
    }
}

[CreateAssetMenu(menuName = "Combat/Coin Effects/Add Coin Power By Status")]
public class CoinEffect_AddCoinPowerByStatus : BaseCoinEffect
{
    public StatusEffectType targetStatus = StatusEffectType.Bleed;
    public int divisor = 6;
    public int maxBonus = 2;

    public override void Execute(BattleCharacter attacker, BattleCharacter target, RuntimeSkill skill)
    {
        StatusEffect effect = target.ActiveEffects.Find(e => e.effectType == targetStatus);
        int potency = effect != null ? effect.potency : 0;
        int bonus = Mathf.FloorToInt(potency / divisor);
        bonus = Mathf.Min(bonus, maxBonus);

        if (bonus > 0)
            skill.CurrentCoinPower += bonus;
    }
}

[CreateAssetMenu(menuName = "Combat/Coin Effects/Amplify Damage By Debuffs")]
public class CoinEffect_AmplifyDamageByDebuffs : BaseCoinEffect
{
    public float bonusPerDebuff = 0.1f;
    public float maxBonus = 1.0f;

    public override void Execute(BattleCharacter attacker, BattleCharacter target, RuntimeSkill skill)
    {
        int debuffCount = 0;
        StatusEffectType[] debuffs = { StatusEffectType.Burn, StatusEffectType.Bleed, StatusEffectType.Tremor, StatusEffectType.Rupture, StatusEffectType.Sinking };

        foreach (var debuff in debuffs)
        {
            var effect = target.ActiveEffects.Find(e => e.effectType == debuff);
            if (effect != null && effect.count > 0 && effect.potency > 0) debuffCount++;
        }

        float totalBonus = Mathf.Min(debuffCount * bonusPerDebuff, maxBonus);
        skill.DamageMultiplierBonus += totalBonus;
    }
}