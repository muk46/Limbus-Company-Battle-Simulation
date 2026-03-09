using UnityEngine;
using System.Collections;

public class BattleClashState : IBattleState
{
    private BattleManager  _manager;
    private BattleCharacter _leftChar,  _rightChar;
    private RuntimeSkill    _leftSkill, _rightSkill;

    public BattleClashState(
        BattleManager manager,
        BattleCharacter leftChar,  RuntimeSkill leftSkill,
        BattleCharacter rightChar, RuntimeSkill rightSkill)
    {
        _manager    = manager;
        _leftChar   = leftChar;
        _leftSkill  = leftSkill;
        _rightChar  = rightChar;
        _rightSkill = rightSkill;
    }

    public void Enter()
    {
        Debug.Log($"[System] {_leftChar.OriginData.characterName} vs {_rightChar.OriginData.characterName} 클래시 시작!");
        _manager.RunRoutine(ClashRoutine());
    }

    public void Execute() { }
    public void Exit()    { }

    private IEnumerator ClashRoutine()
    {
        yield return new WaitForSeconds(1.0f);

        int leftCoins  = _leftSkill.OriginData.coinCount;
        int rightCoins = _rightSkill.OriginData.coinCount;
        int clashCount = 0;

        while (leftCoins > 0 && rightCoins > 0)
        {
            clashCount++;
            var round = ClashCalculator.ProcessClashRound(
                _leftChar,  _leftSkill,  leftCoins,
                _rightChar, _rightSkill, rightCoins);

            Debug.Log($"[System] {clashCount}번째 클래시\n" +
                      $"{_leftChar.OriginData.characterName} ({round.leftCoinLog}) 파워: {round.leftPower}\n" +
                      $"{_rightChar.OriginData.characterName} ({round.rightCoinLog}) 파워: {round.rightPower}");

            BattleEventBus.Raise_OnClashRoundResolved(clashCount, round.leftPower, round.rightPower);

            yield return new WaitForSeconds(0.8f);

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
                Debug.Log("[System] 무승부! 코인이 파괴되지 않고 다시 진행합니다.");
            }

            yield return new WaitForSeconds(0.5f);
        }

        // 클래시 승자 결정 및 데미지 적용
        if (leftCoins > 0)
        {
            Debug.Log($"[System] 클래시 승자: {_leftChar.OriginData.characterName}! 남은 코인 {leftCoins}개로 타격합니다.");
            BattleEventBus.Raise_OnClashWin(_leftChar, _rightChar, clashCount);
            ClashCalculator.ResolveClash(_leftChar, _leftSkill, _rightChar, clashCount, leftCoins);
        }
        else
        {
            Debug.Log($"[System] 클래시 승자: {_rightChar.OriginData.characterName}! 남은 코인 {rightCoins}개로 타격합니다.");
            BattleEventBus.Raise_OnClashWin(_rightChar, _leftChar, clashCount);
            ClashCalculator.ResolveClash(_rightChar, _rightSkill, _leftChar, clashCount, rightCoins);
        }

        yield return new WaitForSeconds(1.0f);

        _manager.ChangeState(new BattleExecuteState(_manager));
    }
}
