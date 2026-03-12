// BattleWaitState.cs 덮어쓰기

using UnityEngine;
using UnityEngine.InputSystem;

public class BattleWaitState : IBattleState
{
    private BattleManager _manager;

    public BattleWaitState(BattleManager manager)
    {
        _manager = manager;
    }

    public void Enter()
    {
        Debug.Log("[System] Wait State 진입: 속도 굴림 완료. 스킬을 드래그하여 타겟을 지정하십시오.");

        foreach (var entity in _manager.PlayerEntities) { entity.Model.RollSpeed(); entity.UpdateUI(); }
        foreach (var entity in _manager.EnemyEntities) { entity.Model.RollSpeed(); entity.UpdateUI(); }

        // [변경점] 턴 시작 즉시 적군의 행동을 굴리고 타겟팅 화살표를 렌더링
        _manager.GenerateEnemyActions();

        _manager.Events.OnEnterWait?.Invoke();
    }

    public void Execute()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (_manager.ActionQueue.Count > 0)
            {
                // [변경점] 전투 실행 시 화면에 띄워둔 적군의 타겟팅 선을 모두 숨김
                foreach (var entity in _manager.EnemyEntities) entity.HideTargetingLine();

                _manager.ChangeState(new BattleExecuteState(_manager));
            }
            else
            {
                Debug.LogWarning("[System] 등록된 행동이 없습니다. 타겟을 먼저 지정하십시오.");
            }
        }
    }

    public void Exit() { }
}
