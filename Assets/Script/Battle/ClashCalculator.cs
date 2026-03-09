using UnityEngine;

/// <summary>
/// 클래시 로직만 담당.
/// 코인 파워 계산(ProcessClashRound)과 결과 적용(ResolveClash)을 처리합니다.
/// 순수 수학은 DamageCalculator, 타격 처리는 HitProcessor에 위임합니다.
/// </summary>
public static class ClashCalculator
{
    public static ClashRoundResult ProcessClashRound(
        BattleCharacter leftChar,  RuntimeSkill leftSkill,  int leftRemainingCoins,
        BattleCharacter rightChar, RuntimeSkill rightSkill, int rightRemainingCoins)
    {
        ClashRoundResult result = new ClashRoundResult();

        int leftBase  = RollAndCalculateBasePower(leftChar,  leftSkill,  leftRemainingCoins,  out result.leftCoinLog);
        int rightBase = RollAndCalculateBasePower(rightChar, rightSkill, rightRemainingCoins, out result.rightCoinLog);

        // 공격 레벨 차이 보정
        int leftLevel  = leftSkill.OriginData.attackLevel  + leftSkill.ResonanceAttackLevelBonus;
        int rightLevel = rightSkill.OriginData.attackLevel + rightSkill.ResonanceAttackLevelBonus;
        int levelDiff  = leftLevel - rightLevel;

        result.leftPower  = leftBase  + (levelDiff > 0 ? levelDiff / 3 : 0);
        result.rightPower = rightBase + (levelDiff < 0 ? Mathf.Abs(levelDiff) / 3 : 0);

        if      (result.leftPower  > result.rightPower) result.winnerFlag = 1;
        else if (result.rightPower > result.leftPower)  result.winnerFlag = 2;
        else                                            result.winnerFlag = 0;

        return result;
    }

    private static int RollAndCalculateBasePower(
        BattleCharacter character, RuntimeSkill skill, int remainingCoins, out string coinLog)
    {
        int headsCount = 0;
        coinLog = "";
        int prob = Mathf.Clamp(50 + character.CurrentSanity, 5, 95);

        for (int i = 0; i < remainingCoins; i++)
        {
            bool isHeads = Random.Range(0, 100) < prob;
            if (isHeads) headsCount++;
            coinLog += isHeads ? "<color=yellow>앞</color> " : "<color=white>뒤</color> ";
        }

        return skill.CurrentBasePower + (headsCount * skill.CurrentCoinPower) + skill.ClashPowerModifier;
    }

    public static void ResolveClash(
        BattleCharacter attacker, RuntimeSkill skill, BattleCharacter target,
        int clashCount = 0, int remainingCoins = -1)
    {
        if (remainingCoins == -1) remainingCoins = skill.OriginData.coinCount;

        bool[] coinResults = HitProcessor.RollCoins(attacker, remainingCoins);
        int damage = HitProcessor.ProcessHit(attacker, target, skill, coinResults, clashCount);

        attacker.TriggerHook(listener => listener.OnSucceedAttack(attacker, target, damage));
        BattleEventBus.Raise_OnHit(attacker, target, skill, damage);

        Debug.Log($"[System] {attacker.OriginData.characterName} → {target.OriginData.characterName} 총 피해 {damage} (남은 체력: {target.CurrentHealth})");
    }
}
