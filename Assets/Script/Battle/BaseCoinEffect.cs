using UnityEngine;
using System.Collections.Generic;

public abstract class BaseCoinEffect : ScriptableObject
{
    [Header("발동 조건")]
    public EffectCondition triggerCondition;

    public abstract void Execute(BattleCharacter attacker, BattleCharacter target, RuntimeSkill skill);
}

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
