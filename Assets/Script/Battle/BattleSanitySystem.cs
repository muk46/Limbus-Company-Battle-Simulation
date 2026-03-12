using UnityEngine;
using System.Collections.Generic;

// 전투 중 발생하는 이벤트를 감지하여 림버스 컴퍼니식 정신력 증감 룰을 적용하는 전담 시스템
public class BattleSanitySystem : MonoBehaviour
{
    // 수정됨: OnEnable -> Start
    private void Start()
    {
        if (BattleEventManager.Instance != null)
        {
            BattleEventManager.Instance.OnClashWin += HandleClashWin;
            BattleEventManager.Instance.OnCharacterDeath += HandleCharacterDeath;
        }
        else
        {
            Debug.LogError("[System] BattleEventManager 인스턴스가 없습니다! 씬에 배치되었는지 확인하세요.");
        }
    }

    // 수정됨: OnDisable -> OnDestroy
    private void OnDestroy()
    {
        if (BattleEventManager.Instance != null)
        {
            BattleEventManager.Instance.OnClashWin -= HandleClashWin;
            BattleEventManager.Instance.OnCharacterDeath -= HandleCharacterDeath;
        }
    }

    // -----------------------------------------------------
    // 1. 합 승리 룰
    // -----------------------------------------------------
    private void HandleClashWin(BattleCharacter winner, BattleCharacter loser, int clashCount)
    {
        if (winner.CurrentHealth <= 0) return;

        // 산식: 기본 10 + (2합부터 1합당 20% 증가)
        // 10의 20%는 2이므로, 10 + (clashCount - 1) * 2 와 동일합니다.
        int sanityRecovery = 10 + ((clashCount - 1) * 2);

        winner.ChangeSanity(sanityRecovery);
        Debug.Log($"[Sanity] 합 승리({clashCount}합): {winner.OriginData.characterName}의 정신력 {sanityRecovery} 증가");
    }

    // -----------------------------------------------------
    // 처치 및 사망 룰 분배기
    // -----------------------------------------------------
    private void HandleCharacterDeath(BattleCharacter deadUnit, BattleCharacter killer)
    {
        bool isEnemyDead = BattleManager.Instance.EnemyTeam.Contains(deadUnit);

        // 1) 적이 죽었을 경우 (처치자 및 아군 보너스 판별)
        if (isEnemyDead && killer != null)
        {
            ProcessEnemyKillBonus(deadUnit, killer);
        }

        // 2) 아군(플레이어)이 죽었을 경우 (남은 아군 패널티 판별)
        if (!isEnemyDead)
        {
            ProcessAllyDeathPenalty(deadUnit);
        }
    }

    // -----------------------------------------------------
    // 2 & 3. 적 처치 및 아군 처치 보너스 룰
    // -----------------------------------------------------
    private void ProcessEnemyKillBonus(BattleCharacter deadEnemy, BattleCharacter killer)
    {
        List<BattleCharacter> playerTeam = BattleManager.Instance.PlayerTeam;

        foreach (var ally in playerTeam)
        {
            if (ally.CurrentHealth <= 0) continue;

            // 처치한 적의 레벨이 자신의 레벨 이상인지 확인
            if (deadEnemy.OriginData.level >= ally.OriginData.level)
            {
                if (ally == killer)
                {
                    // 직접 처치한 경우
                    ally.ChangeSanity(10);
                    Debug.Log($"[Sanity] 강적 처치: {ally.OriginData.characterName}의 정신력 10 증가");
                }
                else
                {
                    // 아군이 처치한 경우
                    ally.ChangeSanity(5);
                    Debug.Log($"[Sanity] 아군의 강적 처치: {ally.OriginData.characterName}의 정신력 5 증가");
                }
            }
        }
    }

    // -----------------------------------------------------
    // 4. 아군 사망 패널티 룰
    // -----------------------------------------------------
    private void ProcessAllyDeathPenalty(BattleCharacter deadAlly)
    {
        List<BattleCharacter> playerTeam = BattleManager.Instance.PlayerTeam;

        foreach (var ally in playerTeam)
        {
            if (ally == deadAlly || ally.CurrentHealth <= 0) continue;

            // 사망한 아군의 레벨이 자신의 레벨 이상인 경우
            if (deadAlly.OriginData.level >= ally.OriginData.level)
            {
                int levelDiff = deadAlly.OriginData.level - ally.OriginData.level;

                // 산식: 기본 10 + 레벨 차이당 10
                int sanityDecrease = 10 + (levelDiff * 10);

                ally.ChangeSanity(-sanityDecrease);
                Debug.Log($"[Sanity] 뛰어난 아군 사망: {ally.OriginData.characterName}의 정신력 {sanityDecrease} 감소 (레벨 차이: {levelDiff})");
            }
        }
    }
}
