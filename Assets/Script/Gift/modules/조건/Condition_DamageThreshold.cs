using UnityEngine;

[CreateAssetMenu(menuName = "Gift/Conditions/Damage Threshold")]
public class Condition_DamageThreshold : GiftCondition
{
    public int minDamage = 12; // 최소 요구 데미지

    public override bool IsMet(BattleCharacter attacker, BattleCharacter target, RuntimeSkill skill, int damage)
    {
        return damage >= minDamage;
    }
}