using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 배틀 이벤트를 구독해 정신력(Sanity) 변화를 처리합니다.
/// BattleEventBus를 통해 클래시 승리 / 캐릭터 사망 이벤트를 받습니다.
/// </summary>
public class BattleSanitySystem : MonoBehaviour
{
    private void Start()
    {
        BattleEventBus.OnClashWin      += HandleClashWin;
        BattleEventBus.OnCharacterDeath += HandleCharacterDeath;
    }

    private void OnDestroy()
    {
        BattleEventBus.OnClashWin      -= HandleClashWin;
        BattleEventBus.OnCharacterDeath -= HandleCharacterDeath;
    }

    // ── 클래시 승리 ──────────────────────────────────────

    private void HandleClashWin(BattleCharacter winner, BattleCharacter loser, int clashCount)
    {
        if (winner.CurrentHealth <= 0) return;

        int recovery = 10 + ((clashCount - 1) * 2);
        winner.ChangeSanity(recovery);
        Debug.Log($"[Sanity] 클래시 승리({clashCount}번): {winner.OriginData.characterName}의 정신력 {recovery} 회복");
    }

    // ── 캐릭터 사망 ──────────────────────────────────────

    private void HandleCharacterDeath(BattleCharacter deadUnit, BattleCharacter killer)
    {
        bool isEnemyDead = BattleManager.Instance.EnemyTeam.Contains(deadUnit);

        if (isEnemyDead && killer != null)
            ProcessEnemyKillBonus(deadUnit, killer);

        if (!isEnemyDead)
            ProcessAllyDeathPenalty(deadUnit);
    }

    private void ProcessEnemyKillBonus(BattleCharacter deadEnemy, BattleCharacter killer)
    {
        List<BattleCharacter> playerTeam = BattleManager.Instance.PlayerTeam;

        foreach (var ally in playerTeam)
        {
            if (ally.CurrentHealth <= 0) continue;
            if (deadEnemy.OriginData.level < ally.OriginData.level) continue;

            int gain = (ally == killer) ? 10 : 5;
            ally.ChangeSanity(gain);
            Debug.Log($"[Sanity] {(ally == killer ? "직접" : "동료")} 적 처치: {ally.OriginData.characterName}의 정신력 {gain} 회복");
        }
    }

    private void ProcessAllyDeathPenalty(BattleCharacter deadAlly)
    {
        List<BattleCharacter> playerTeam = BattleManager.Instance.PlayerTeam;

        foreach (var ally in playerTeam)
        {
            if (ally == deadAlly || ally.CurrentHealth <= 0) continue;
            if (deadAlly.OriginData.level < ally.OriginData.level) continue;

            int levelDiff   = deadAlly.OriginData.level - ally.OriginData.level;
            int penalty     = 10 + (levelDiff * 10);
            ally.ChangeSanity(-penalty);
            Debug.Log($"[Sanity] 동료 사망 패널티: {ally.OriginData.characterName}의 정신력 {penalty} 감소 (레벨 차이: {levelDiff})");
        }
    }
}
