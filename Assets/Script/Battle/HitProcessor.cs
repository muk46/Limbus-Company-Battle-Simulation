using UnityEngine;

/// <summary>
/// 실제 타격 처리를 담당.
/// 코인 굴리기, 코인별 데미지 적용, 상태이상 발동, 코인 이펙트 트리거를 처리합니다.
/// </summary>
public static class HitProcessor
{
    /// <summary>
    /// 공격자의 코인을 굴립니다. 출혈(Bleed)처럼 코인 굴릴 때 발동하는 상태이상도 처리합니다.
    /// </summary>
    public static bool[] RollCoins(BattleCharacter attacker, int coinCount)
    {
        bool[] results = new bool[coinCount];
        int prob = Mathf.Clamp(50 + attacker.CurrentSanity, 5, 95);
        string log = "";

        StatusEffect bleed = attacker.ActiveEffects.Find(e => e.effectType == StatusEffectType.Bleed);
        StatusEffectData bleedData = StatusEffectDatabase.Instance?.Get(StatusEffectType.Bleed);

        for (int i = 0; i < coinCount; i++)
        {
            bool isHeads = Random.Range(0, 100) < prob;
            results[i] = isHeads;
            log += isHeads ? "<color=yellow>앞</color> " : "<color=white>뒤</color> ";

            if (bleed != null)
                bleedData?.OnCoinRoll(attacker, bleed);
        }

        Debug.Log($"[Coin] {attacker.OriginData.characterName} 코인 결과: {log}");
        return results;
    }

    /// <summary>
    /// 코인 결과를 기반으로 코인별 데미지를 계산·적용하고 총 피해량을 반환합니다.
    /// BeforeHit / OnHit 이펙트 및 타격 상태이상(Rupture, Sinking)도 여기서 처리합니다.
    /// </summary>
    public static int ProcessHit(
        BattleCharacter attacker, BattleCharacter target, RuntimeSkill skill,
        bool[] coinResults, int clashCount = 0)
    {
        int totalDamage = 0;
        float baseMultiplier = DamageCalculator.GetBaseMultiplier(attacker, target, skill, clashCount);
        int currentPower = skill.CurrentBasePower;

        StatusEffect poise = attacker.ActiveEffects.Find(e => e.effectType == StatusEffectType.Poise);
        StatusEffectData poiseData = StatusEffectDatabase.Instance?.Get(StatusEffectType.Poise);

        for (int i = 0; i < coinResults.Length; i++)
        {
            if (coinResults[i]) currentPower += skill.CurrentCoinPower;

            int finalCoinPower = currentPower + skill.FinalPowerModifier;
            string coinSide = coinResults[i] ? "<color=yellow>앞</color>" : "<color=white>뒤</color>";

            // A. BeforeHit 이펙트 (데미지 배율 수정)
            skill.DamageMultiplierBonus = 0f;
            TriggerCoinEffects(attacker, target, skill, i, EffectCondition.BeforeHit);

            // B. 크리티컬 배율 (호기)
            float critMultiplier = (poise != null)
                ? poiseData?.GetCritMultiplier(attacker, poise) ?? 1f
                : 1f;

            // C. 최종 데미지 계산
            float finalMultiplier = baseMultiplier + skill.DamageMultiplierBonus;
            int coinDamage = Mathf.FloorToInt(finalCoinPower * finalMultiplier * critMultiplier);
            totalDamage += coinDamage;

            Debug.Log($"[Hit] {i + 1}타 ({coinSide}): 파워 {finalCoinPower} | 배율 {finalMultiplier:F2} | <color=red>피해 {coinDamage}</color>");

            // D. 타격 적용
            target.TakeDamage(coinDamage, attacker);

            // E. 타격 상태이상 발동 (Rupture, Sinking 등)
            TriggerOnHitStatusEffects(attacker, target);

            // F. OnHit / OnHeadsHit / OnTailsHit 코인 이펙트
            TriggerCoinEffects(attacker, target, skill, i, EffectCondition.OnHit);
            if (coinResults[i]) TriggerCoinEffects(attacker, target, skill, i, EffectCondition.OnHeadsHit);
            else                TriggerCoinEffects(attacker, target, skill, i, EffectCondition.OnTailsHit);
        }

        return Mathf.Max(1, totalDamage);
    }

    private static void TriggerOnHitStatusEffects(BattleCharacter attacker, BattleCharacter target)
    {
        foreach (var effect in target.ActiveEffects)
        {
            if (effect.count <= 0) continue;
            StatusEffectData data = StatusEffectDatabase.Instance?.Get(effect.effectType);
            data?.OnHit(attacker, target, effect);
        }
    }

    private static void TriggerCoinEffects(
        BattleCharacter attacker, BattleCharacter target, RuntimeSkill skill,
        int coinIndex, EffectCondition condition)
    {
        if (skill.OriginData.coins == null || coinIndex >= skill.OriginData.coins.Count) return;

        var coinData = skill.OriginData.coins[coinIndex];
        if (coinData?.coinEffects == null) return;

        foreach (var effect in coinData.coinEffects)
        {
            if (effect == null) continue;
            if (effect.triggerCondition == condition)
                effect.Execute(attacker, target, skill);
        }
    }
}
