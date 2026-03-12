using UnityEngine;
using System.Collections;

public class BattleClashState : IBattleState
{
    private BattleManager _manager;
    private BattleCharacter _leftChar, _rightChar;
    private RuntimeSkill _leftSkill, _rightSkill;

    public BattleClashState(BattleManager manager, BattleCharacter leftChar, RuntimeSkill leftSkill, BattleCharacter rightChar, RuntimeSkill rightSkill)
    {
        _manager = manager;
        _leftChar = leftChar;
        _leftSkill = leftSkill;
        _rightChar = rightChar;
        _rightSkill = rightSkill;
    }

    public void Enter()
    {
        Debug.Log($"[System] {_leftChar.OriginData.characterName} vs {_rightChar.OriginData.characterName} 합(Clash) 돌입!");
        _manager.RunRoutine(ClashRoutine());
    }

    public void Execute() { }
    public void Exit() { }

    private IEnumerator ClashRoutine()
    {
        yield return new WaitForSeconds(1.0f); // 합 돌입 전 대기 딜레이

        int leftCoins = _leftSkill.OriginData.coinCount;
        int rightCoins = _rightSkill.OriginData.coinCount;
        int clashCount = 0;

        // 양측 중 한 명의 코인이 파괴되어 0이 될 때까지 반복
        while (leftCoins > 0 && rightCoins > 0)
        {
            clashCount++;
            var round = ClashCalculator.ProcessClashRound(_leftChar, _leftSkill, leftCoins, _rightChar, _rightSkill, rightCoins);

            Debug.Log($"[System] {clashCount}합 진행\n" +
                      $"{_leftChar.OriginData.characterName} ({round.leftCoinLog}) 위력: {round.leftPower}\n" +
                      $"{_rightChar.OriginData.characterName} ({round.rightCoinLog}) 위력: {round.rightPower}");

            yield return new WaitForSeconds(0.8f); // 위력 확인용 시각적 딜레이

            if (round.winnerFlag == 1)
            {
                rightCoins--;
                Debug.Log($"[System] {_rightChar.OriginData.characterName}의 코인이 파괴되었습니다. (남은 코인: {rightCoins})");
            }
            else if (round.winnerFlag == 2)
            {
                leftCoins--;
                Debug.Log($"[System] {_leftChar.OriginData.characterName}의 코인이 파괴되었습니다. (남은 코인: {leftCoins})");
            }
            else
            {
                Debug.Log("[System] 합 무승부! 코인이 파괴되지 않고 다시 굴립니다.");
            }

            yield return new WaitForSeconds(0.5f); // 코인 파괴 여부 확인용 딜레이
        }

        // 최종 승자 도출 및 타격으로 전환 (남은 코인 개수를 넘겨줌)
        if (leftCoins > 0)
        {
            Debug.Log($"[System] 최종 합 승리: {_leftChar.OriginData.characterName}! 남은 코인 {leftCoins}개로 타격을 시작합니다.");

            // [추가] 합 승리 이벤트 발생
            if (BattleEventManager.Instance != null)
                BattleEventManager.Instance.TriggerClashWin(_leftChar, _rightChar, clashCount);

            ClashCalculator.ResolveClash(_leftChar, _leftSkill, _rightChar, clashCount, leftCoins);
        }
        else
        {
            Debug.Log($"[System] 최종 합 승리: {_rightChar.OriginData.characterName}! 남은 코인 {rightCoins}개로 타격을 시작합니다.");

            // [추가] 합 승리 이벤트 발생
            if (BattleEventManager.Instance != null)
                BattleEventManager.Instance.TriggerClashWin(_rightChar, _leftChar, clashCount);

            ClashCalculator.ResolveClash(_rightChar, _rightSkill, _leftChar, clashCount, rightCoins);
        }
    }
}
