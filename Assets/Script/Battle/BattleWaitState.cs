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
        Debug.Log("[System] Wait State 진입: 스킬을 드래그하여 타겟을 선택하십시오.");

        foreach (var entity in _manager.PlayerEntities)
        {
            entity.Model.RecoverStagger();
            entity.Model.RollSpeed();
            entity.UpdateUI();
        }
        foreach (var entity in _manager.EnemyEntities)
        {
            entity.Model.RecoverStagger();
            entity.Model.RollSpeed();
            entity.UpdateUI();
        }

        _manager.GenerateEnemyActions();
        BattleEventBus.Raise_OnEnterWait();
    }

    public void Execute()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (_manager.ActionQueue.Count > 0)
            {
                foreach (var entity in _manager.EnemyEntities) entity.HideTargetingLine();
                BattleEventBus.Raise_OnExitWait();
                _manager.ChangeState(new BattleExecuteState(_manager));
            }
            else
            {
                Debug.LogWarning("[System] 등록된 행동이 없습니다. 타겟을 먼저 선택하십시오.");
            }
        }
    }

    public void Exit() { }
}
