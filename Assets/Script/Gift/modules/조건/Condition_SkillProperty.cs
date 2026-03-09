using UnityEngine;

[CreateAssetMenu(menuName = "Gift/Conditions/Skill Property")]
public class Condition_SkillProperty : GiftCondition
{
    [Header("공격 타입 검사")]
    public bool checkDamageType;
    public DamageType requiredDamageType;

    [Header("죄악 속성 검사")]
    public bool checkSinAffinity;
    public SinAffinity requiredSinAffinity;

    public override bool IsMet(BattleCharacter attacker, BattleCharacter target, RuntimeSkill skill, int damage)
    {
        if (skill == null || skill.OriginData == null) return false;

        if (checkDamageType && skill.OriginData.damageType != requiredDamageType) return false;
        if (checkSinAffinity && skill.OriginData.sinAffinity != requiredSinAffinity) return false;

        return true;
    }
}