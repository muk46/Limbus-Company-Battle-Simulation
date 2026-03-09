using UnityEngine;

/// <summary>
/// 순수 수학 연산만 담당. 사이드 이펙트 없음.
/// 저항, 레벨 보정, 최종 배율 계산을 제공합니다.
/// </summary>
public static class DamageCalculator
{
    public static float GetLevelCorrection(int attackerOffLevel, int targetDefLevel)
    {
        float diff = attackerOffLevel - targetDefLevel;
        return diff / (25f + Mathf.Abs(diff));
    }

    public static float GetResistanceCorrection(float physicalRes, float sinRes)
    {
        float physBonus = Mathf.Max(physicalRes - 1f, (physicalRes - 1f) * 0.5f);
        float sinBonus  = Mathf.Max(sinRes - 1f,      (sinRes - 1f)      * 0.5f);
        return physBonus + sinBonus;
    }

    public static float GetPhysicalResistance(BattleCharacter target, RuntimeSkill skill)
    {
        if (target.StaggerLevel >= 3) return 3f;
        if (target.StaggerLevel == 2) return 2.5f;
        if (target.StaggerLevel == 1) return 2f;

        switch (skill.OriginData.damageType)
        {
            case DamageType.Slash:  return target.OriginData.slashRes;
            case DamageType.Pierce: return target.OriginData.pierceRes;
            case DamageType.Blunt:  return target.OriginData.bluntRes;
        }
        return 1f;
    }

    public static float GetSinResistance(BattleCharacter target, RuntimeSkill skill)
    {
        switch (skill.OriginData.sinAffinity)
        {
            case SinAffinity.Wrath:    return target.OriginData.wrathRes;
            case SinAffinity.Lust:     return target.OriginData.lustRes;
            case SinAffinity.Sloth:    return target.OriginData.slothRes;
            case SinAffinity.Gluttony: return target.OriginData.gluttonyRes;
            case SinAffinity.Gloom:    return target.OriginData.gloomRes;
            case SinAffinity.Pride:    return target.OriginData.prideRes;
            case SinAffinity.Envy:     return target.OriginData.envyRes;
        }
        return 1f;
    }

    /// <summary>저항 + 레벨 보정 + 클래시 보정을 합산한 최종 기본 배율을 반환합니다.</summary>
    public static float GetBaseMultiplier(BattleCharacter attacker, BattleCharacter target, RuntimeSkill skill, int clashCount = 0)
    {
        float physRes  = GetPhysicalResistance(target, skill);
        float sinRes   = GetSinResistance(target, skill);
        float resCor   = GetResistanceCorrection(physRes, sinRes);
        float levelCor = GetLevelCorrection(
            skill.OriginData.attackLevel + skill.ResonanceAttackLevelBonus,
            target.OriginData.defenseLevel);
        float clashCor = clashCount * 0.03f;

        return 1f + resCor + levelCor + clashCor;
    }
}
