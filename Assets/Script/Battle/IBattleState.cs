public interface IBattleState
{
    void Enter();    // 상태 진입 시 1회 호출 (초기화, UI 연출 트리거)
    void Execute();  // 상태 지속 중 로직 처리 (선택 완료 대기, 연산 진행 등)
    void Exit();     // 상태 종료 시 1회 호출 (메모리 정리, 다음 상태 전이 준비)
}