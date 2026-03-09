using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class ResonanceCalculator
{
    /// <summary>
    /// playerTeam을 인자로 받아 싱글톤 의존성을 제거했습니다.
    /// BattleExecuteState에서 _manager.PlayerTeam을 전달합니다.
    /// </summary>
    public static void ApplyResonance(List<BattleAction> actionQueue, List<BattleCharacter> playerTeam)
    {
        var playerActions = actionQueue
            .Where(a => a.Attacker != null && playerTeam.Contains(a.Attacker))
            .ToList();

        if (playerActions.Count == 0) return;

        // 레조넌스 보너스 초기화
        foreach (var action in playerActions)
            action.UsedSkill.ResonanceAttackLevelBonus = 0;

        // 1. 일반 레조넌스 (같은 죄악 2번째부터 +1씩)
        var sinCounts = new Dictionary<SinAffinity, int>();
        for (int i = 0; i < playerActions.Count; i++)
        {
            var sin = playerActions[i].UsedSkill.OriginData.sinAffinity;
            if (!sinCounts.ContainsKey(sin)) sinCounts[sin] = 0;
            sinCounts[sin]++;

            if (sinCounts[sin] >= 2)
            {
                playerActions[i].UsedSkill.ResonanceAttackLevelBonus = sinCounts[sin] - 1;
                Debug.Log($"[Resonance] {sin} 레조넌스 발동! ({playerActions[i].Attacker.OriginData.characterName} 공격 레벨 +{sinCounts[sin] - 1})");
            }
        }

        // 2. 절대 레조넌스 (3개 이상 연속 배치 시 일괄 최대치 부여)
        int consecutiveCount = 1;
        SinAffinity? prevSin = null;
        int startIndex = 0;

        for (int i = 0; i < playerActions.Count; i++)
        {
            var sin = playerActions[i].UsedSkill.OriginData.sinAffinity;

            if (prevSin == sin)
            {
                consecutiveCount++;
            }
            else
            {
                if (consecutiveCount >= 3 && prevSin.HasValue)
                    ApplyAbsoluteResonance(playerActions, startIndex, consecutiveCount, prevSin.Value);

                consecutiveCount = 1;
                startIndex = i;
            }
            prevSin = sin;
        }

        if (consecutiveCount >= 3 && prevSin.HasValue)
            ApplyAbsoluteResonance(playerActions, startIndex, consecutiveCount, prevSin.Value);
    }

    private static void ApplyAbsoluteResonance(List<BattleAction> actions, int startIndex, int count, SinAffinity sin)
    {
        int maxBonus = count - 1;
        Debug.Log($"[Resonance] {sin} 절대 레조넌스 발동! (연속 {count}체인, 해당 스킬 모두 공격 레벨 +{maxBonus})");

        for (int i = startIndex; i < startIndex + count; i++)
            actions[i].UsedSkill.ResonanceAttackLevelBonus = maxBonus;
    }
}
