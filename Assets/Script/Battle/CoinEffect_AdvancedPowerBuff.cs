using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Combat/Coin Effects/Advanced Power Buff")]
public class CoinEffect_AdvancedPowerBuff : BaseCoinEffect
{
    [Header("발동 조건 리스트")]
    public List<CoinCondition> conditions = new List<CoinCondition>();
    [Header("위력 보정 설정")]
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
