using UnityEngine;

[CreateAssetMenu(menuName = "Gift/Conditions/Target Status")]
public class Condition_TargetStatus : GiftCondition
{
    public bool checkSelf = false; // true: 자신 검사, false: 타겟 검사
    public StatusEffectType requiredEffect;

    [Header("위력 및 횟수 역치")]
    public int minPotency = 0;
    public int maxPotency = 9999; // 상한선 검사 (예: 15 미만)
    public int minCount = 0;

    public override bool IsMet(BattleCharacter attacker, BattleCharacter target, RuntimeSkill skill, int damage)
    {
        BattleCharacter targetToCheck = checkSelf ? attacker : target;
        if (targetToCheck == null) return false;

        StatusEffect effect = targetToCheck.ActiveEffects.Find(e => e.effectType == requiredEffect);

        // 상태이상이 아예 없는 경우
        if (effect == null)
        {
            // 요구치가 0 이하라면 없는 것도 통과로 간주할 수 있으나, 일반적으로는 false
            return minPotency <= 0 && minCount <= 0;
        }

        // 수치 검사
        if (effect.potency < minPotency || effect.potency > maxPotency) return false;
        if (effect.count < minCount) return false;

        return true;
    }
}