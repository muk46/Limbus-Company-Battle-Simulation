using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class ResonanceCalculator
{
    public static void ApplyResonance(List<BattleAction> actionQueue, List<BattleCharacter> playerTeam)
    {
        // 아군(플레이어)이 큐에 등록한 스킬 순서대로 필터링
        List<BattleAction> playerActions = actionQueue
            .Where(a => a.Attacker != null && playerTeam.Contains(a.Attacker))
            .ToList();

        if (playerActions.Count == 0) return;

        // 보너스 초기화 및 죄악별 등장 횟수 카운트
        Dictionary<SinAffinity, int> sinCounts = new Dictionary<SinAffinity, int>();

        foreach (var action in playerActions)
        {
            action.UsedSkill.ResonanceAttackLevelBonus = 0;
        }

        // 1. 일반 공명 연산 (동일 죄악 등장 시 누적 +1)
        for (int i = 0; i < playerActions.Count; i++)
        {
            SinAffinity currentSin = playerActions[i].UsedSkill.OriginData.sinAffinity;

            if (!sinCounts.ContainsKey(currentSin))
                sinCounts[currentSin] = 0;

            sinCounts[currentSin]++;
            int count = sinCounts[currentSin];

            if (count >= 2)
            {
                playerActions[i].UsedSkill.ResonanceAttackLevelBonus = count - 1;
                Debug.Log($"[Resonance] {currentSin} 죄악 공명 발동! ({playerActions[i].Attacker.OriginData.characterName} 공격 레벨 +{count - 1})");
            }
        }

        // 2. 완전 공명 연산 (3개 이상 연속 배치 시 일괄 최대치 부여)
        int consecutiveCount = 1;
        SinAffinity? prevSin = null;
        int startIndex = 0;

        for (int i = 0; i < playerActions.Count; i++)
        {
            SinAffinity currentSin = playerActions[i].UsedSkill.OriginData.sinAffinity;

            if (prevSin == currentSin)
            {
                consecutiveCount++;
            }
            else
            {
                if (consecutiveCount >= 3 && prevSin.HasValue)
                {
                    ApplyAbsoluteResonance(playerActions, startIndex, consecutiveCount, prevSin.Value);
                }
                consecutiveCount = 1;
                startIndex = i;
            }
            prevSin = currentSin;
        }

        // 마지막 체인 블록 검사
        if (consecutiveCount >= 3 && prevSin.HasValue)
        {
            ApplyAbsoluteResonance(playerActions, startIndex, consecutiveCount, prevSin.Value);
        }
    }

    private static void ApplyAbsoluteResonance(List<BattleAction> actions, int startIndex, int count, SinAffinity sin)
    {
        int maxBonus = count - 1;
        Debug.Log($"[Resonance] {sin} 완전 공명 발동! (연속 {count}체인, 연관 스킬 모두 공격 레벨 +{maxBonus})");

        for (int i = startIndex; i < startIndex + count; i++)
        {
            actions[i].UsedSkill.ResonanceAttackLevelBonus = maxBonus;
        }
    }
}