using UnityEngine;

public class BattleInitState : IBattleState
{
    private readonly BattleManager _manager;

    public BattleInitState(BattleManager manager)
    {
        _manager = manager;
    }

    public void Enter()
    {
        Debug.Log("[System] Init State 진입");
        // 초기 데이터 세팅은 BattleManager.Start()에서 완료됨.
        // 바로 대기(Wait) 상태로 전환합니다.
        _manager.ChangeState(new BattleWaitState(_manager));
    }

    public void Execute() { }

    public void Exit()
    {
        Debug.Log("[System] Init State 종료");
    }
}