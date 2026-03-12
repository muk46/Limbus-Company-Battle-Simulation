using UnityEngine;

public enum CoinForceMode { Random, AllHeads, AllTails, Custom }

public static class ClashCalculator
{
    public static CoinForceMode ForceCoinMode = CoinForceMode.Random;
    public static bool[] CustomCoinPattern = null;
    private static int _customIndex = 0;

    public static void ResetCustomIndex() { _customIndex = 0; }

    private static bool FlipCoin(int headsProbability, bool useCustom = false)
    {
        switch (ForceCoinMode)
        {
            case CoinForceMode.AllHeads: return true;
            case CoinForceMode.AllTails: return false;
            case CoinForceMode.Custom:
                if (useCustom && CustomCoinPattern != null && _customIndex < CustomCoinPattern.Length)
                    return CustomCoinPattern[_customIndex++];
                return Random.Range(0, 100) < headsProbability;
            default: return Random.Range(0, 100) < headsProbability;
        }
    }

    private static float GetLevelCorrection(int attackerOffLevel, int targetDefLevel)
    {
        float diff = attackerOffLevel - targetDefLevel;
        return diff / (25f + Mathf.Abs(diff));
    }

    private static float GetResistanceCorrection(float physicalRes, float sinRes)
    {
        float physBonus = Mathf.Max(physicalRes - 1f, (physicalRes - 1f) * 0.5f);
        float sinBonus = Mathf.Max(sinRes - 1f, (sinRes - 1f) * 0.5f);
        return physBonus + sinBonus;
    }

    private static float GetPhysicalResistance(BattleCharacter target, RuntimeSkill skill)
    {
        float res = 1f;
        switch (skill.OriginData.damageType)
        {
            case DamageType.Slash: res = target.OriginData.slashRes; break;
            case DamageType.Pierce: res = target.OriginData.pierceRes; break;
            case DamageType.Blunt: res = target.OriginData.bluntRes; break;
        }

        if (target.StaggerLevel == 1) res = 2f;
        else if (target.StaggerLevel == 2) res = 2.5f;
        else if (target.StaggerLevel >= 3) res = 3f;

        return res;
    }

    private static float GetSinResistance(BattleCharacter target, RuntimeSkill skill)
    {
        switch (skill.OriginData.sinAffinity)
        {
            case SinAffinity.Wrath: return target.OriginData.wrathRes;
            case SinAffinity.Lust: return target.OriginData.lustRes;
            case SinAffinity.Sloth: return target.OriginData.slothRes;
            case SinAffinity.Gluttony: return target.OriginData.gluttonyRes;
            case SinAffinity.Gloom: return target.OriginData.gloomRes;
            case SinAffinity.Pride: return target.OriginData.prideRes;
            case SinAffinity.Envy: return target.OriginData.envyRes;
        }
        return 1f;
    }

    public struct ClashRoundResult
    {
        public int leftPower;
        public int rightPower;
        public string leftCoinLog;
        public string rightCoinLog;
        public int winnerFlag;
    }

    public static ClashRoundResult ProcessClashRound(
        BattleCharacter leftChar, RuntimeSkill leftSkill, int leftRemainingCoins,
        BattleCharacter rightChar, RuntimeSkill rightSkill, int rightRemainingCoins)
    {
        ClashRoundResult result = new ClashRoundResult();

        int leftHeads = 0;
        result.leftCoinLog = "";
        int leftProb = Mathf.Clamp(50 + leftChar.CurrentSanity, 5, 95);

        for (int i = 0; i < leftRemainingCoins; i++)
        {
            bool isHeads = FlipCoin(leftProb);
            if (isHeads) leftHeads++;
            result.leftCoinLog += isHeads ? "<color=yellow>●</color> " : "<color=white>○</color> ";
        }

        int rightHeads = 0;
        result.rightCoinLog = "";
        int rightProb = Mathf.Clamp(50 + rightChar.CurrentSanity, 5, 95);

        for (int i = 0; i < rightRemainingCoins; i++)
        {
            bool isHeads = FlipCoin(rightProb);
            if (isHeads) rightHeads++;
            result.rightCoinLog += isHeads ? "<color=yellow>●</color> " : "<color=white>○</color> ";
        }

        // 1. ProcessClashRound 내부 수정 (이전에 추가했던 공격 레벨 차이 계산 부분)
        int leftAttackLevel = leftSkill.OriginData.attackLevel + leftSkill.ResonanceAttackLevelBonus; // 보너스 추가
        int rightAttackLevel = rightSkill.OriginData.attackLevel + rightSkill.ResonanceAttackLevelBonus; // 보너스 추가
        int levelDiff = leftAttackLevel - rightAttackLevel;

        int leftLevelBonus = levelDiff > 0 ? levelDiff / 3 : 0;
        int rightLevelBonus = levelDiff < 0 ? Mathf.Abs(levelDiff) / 3 : 0;

        // 합 위력(ClashPowerModifier) 및 레벨 보정 적용
        result.leftPower = leftSkill.CurrentBasePower + (leftHeads * leftSkill.CurrentCoinPower) + leftSkill.ClashPowerModifier + leftLevelBonus;
        result.rightPower = rightSkill.CurrentBasePower + (rightHeads * rightSkill.CurrentCoinPower) + rightSkill.ClashPowerModifier + rightLevelBonus;

        if (result.leftPower > result.rightPower) result.winnerFlag = 1;
        else if (result.rightPower > result.leftPower) result.winnerFlag = 2;
        else result.winnerFlag = 0;

        return result;
    }

    private static bool[] RollCoinsForAttack(BattleCharacter attacker, int remainingCoins)
    {
        bool[] results = new bool[remainingCoins];
        int headsProbability = Mathf.Clamp(50 + attacker.CurrentSanity, 5, 95);
        string log = "";

        // [상태이상] 출혈 체크
        StatusEffect bleed = attacker.ActiveEffects.Find(e => e.effectType == StatusEffectType.Bleed);

        for (int i = 0; i < remainingCoins; i++)
        {
            bool isHeads = FlipCoin(headsProbability, useCustom: true);
            results[i] = isHeads;
            log += isHeads ? "<color=yellow>●</color> " : "<color=white>○</color> ";

            // [상태이상] 코인 판정 시 출혈 피해 및 횟수 차감
            if (bleed != null && bleed.count > 0)
            {
                attacker.TakeDamage(bleed.potency);
                Debug.Log($"[StatusEffect] 출혈 발동! {attacker.OriginData.characterName}가 {bleed.potency} 피해를 입음.");
                bleed.count--;
            }
        }

        Debug.Log($"[Coin] {attacker.OriginData.characterName} 타격 코인: {log}");
        return results;
    }

    private static int CalculateDamage(
        BattleCharacter attacker, RuntimeSkill skill, BattleCharacter target,
        bool[] coinResults, int clashCount = 0)
    {
        int totalDamage = 0;
        int basePower = skill.CurrentBasePower;
        int coinPower = skill.CurrentCoinPower;

        // 1. 내성연산 기초값 계산
        float physRes = GetPhysicalResistance(target, skill);
        float sinRes = GetSinResistance(target, skill);
        float resCor = GetResistanceCorrection(physRes, sinRes);
        float levelCor = GetLevelCorrection(skill.OriginData.attackLevel + skill.ResonanceAttackLevelBonus, target.OriginData.defenseLevel);
        float clashCor = clashCount * 0.03f;

        // 내성연산 기본값 (치명타 제외): 1 + 물리내성보정 + 죄악내성보정 + 합횟수보정 + 레벨보정
        float baseResMultiplier = 1f + resCor + clashCor + levelCor;

        int currentPower = basePower;

        // [상태이상] 타격 전 호흡 체크
        StatusEffect poise = attacker.ActiveEffects.Find(e => e.effectType == StatusEffectType.Poise);

        for (int i = 0; i < coinResults.Length; i++)
        {
            // A. 코인 위력 누적 (앞면일 때만)
            if (coinResults[i]) currentPower += coinPower;

            int finalCoinPower = currentPower + skill.FinalPowerModifier;
            string coinSide = coinResults[i] ? "<color=yellow>앞</color>" : "<color=white>뒤</color>";

            // B. 타격 직전(BeforeHit) 이펙트 발동
            skill.DamageMultiplierBonus = 0f;
            if (skill.OriginData.coins != null && i < skill.OriginData.coins.Count)
            {
                var coinData = skill.OriginData.coins[i];
                if (coinData.coinEffects != null)
                {
                    foreach (var effect in coinData.coinEffects)
                    {
                        if (effect.triggerCondition == EffectCondition.BeforeHit)
                            effect.Execute(attacker, target, skill);
                    }
                }
            }

            // C. 호흡 치명타 판정 → 내성연산에 +0.2 합연산
            float critBonus = 0f;
            if (poise != null && poise.count > 0)
            {
                if (Random.Range(0, 100) < (poise.potency * 5))
                {
                    critBonus = 0.2f;
                    poise.count--;
                }
            }

            // D. 최종 피해량 산출
            // 원작 공식: 위력 × 버프연산 × 내성연산
            // 버프연산: 1 + [피해량증가/감소 합산] (패시브, 기프트, 취약, 보호 등)
            float buffMultiplier = 1f + skill.DamageMultiplierBonus;
            // 내성연산: 기본값 + 치명타보정(0.2)
            float resMultiplier = baseResMultiplier + critBonus;

            int coinDamage = Mathf.FloorToInt(finalCoinPower * buffMultiplier * resMultiplier);

            totalDamage += coinDamage;

            // E. 코인당 상세 로그 출력
            Debug.Log($"[Hit] {i + 1}타 ({coinSide}): 위력 {finalCoinPower} | 버프 {buffMultiplier:F2} | 내성 {resMultiplier:F2} | <color=red>피해 {coinDamage}</color>");

            // F. 타격 적중 시(OnHit) 처리
            target.TakeDamage(coinDamage, attacker);

            // 파열 처리 (고정피해)
            StatusEffect rupture = target.ActiveEffects.Find(e => e.effectType == StatusEffectType.Rupture);
            if (rupture != null && rupture.count > 0)
            {
                target.TakeDamage(rupture.potency);
                totalDamage += rupture.potency;
                Debug.Log($"[StatusEffect] 파열 발동! {rupture.potency} 고정 피해.");
                rupture.count--;
            }

            // 침잠 처리 (고정피해)
            StatusEffect sinking = target.ActiveEffects.Find(e => e.effectType == StatusEffectType.Sinking);
            if (sinking != null && sinking.count > 0)
            {
                if (target.OriginData.maxSanity > 0) target.ChangeSanity(-sinking.potency);
                else
                {
                    int sinkingDamage = Mathf.FloorToInt(sinking.potency * target.OriginData.gloomRes);
                    target.TakeDamage(sinkingDamage);
                    totalDamage += sinkingDamage;
                }
                sinking.count--;
            }

            // 코인 이펙트 실행 (OnHit, OnHeadsHit 등)
            if (skill.OriginData.coins != null && i < skill.OriginData.coins.Count)
            {
                var coinData = skill.OriginData.coins[i];
                if (coinData.coinEffects != null)
                {
                    foreach (var effect in coinData.coinEffects)
                    {
                        bool shouldTrigger = effect.triggerCondition == EffectCondition.OnHit ||
                                             (effect.triggerCondition == EffectCondition.OnHeadsHit && coinResults[i]) ||
                                             (effect.triggerCondition == EffectCondition.OnTailsHit && !coinResults[i]);

                        if (shouldTrigger) effect.Execute(attacker, target, skill);
                    }
                }
            }
        }

        return Mathf.Max(1, totalDamage);
    }

    public static void ResolveClash(
        BattleCharacter attacker, RuntimeSkill skill, BattleCharacter target,
        int clashCount = 0, int remainingCoins = -1)
    {
        if (remainingCoins == -1) remainingCoins = skill.OriginData.coinCount;
        bool[] coinResults = RollCoinsForAttack(attacker, remainingCoins);

        // CalculateDamage 내부에서 코인별로 TakeDamage를 이미 처리함
        int damage = CalculateDamage(attacker, skill, target, coinResults, clashCount);

        attacker.TriggerHook(listener => listener.OnSucceedAttack(attacker, target, damage));

        if (BattleEventManager.Instance != null)
        {
            BattleEventManager.Instance.TriggerOnHit(attacker, target, skill, damage);
        }

        Debug.Log($"[System] {attacker.OriginData.characterName} → {target.OriginData.characterName} 총 피해 {damage} (남은 체력: {target.CurrentHealth})");
    }
}
