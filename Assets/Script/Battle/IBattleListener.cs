public interface IBattleListener
{
    // 효과 발동 우선순위 (낮을수록 먼저 실행)
    int Priority { get; }

    // 1. 라운드 시작 시점 (속도 굴림 전 등)
    void OnRoundStart(BattleCharacter owner);

    // 2. 위력 계산 시점 (합/공격 전 위력 보정)
    // ref 키워드를 통해 최종 위력을 직접 수정할 수 있게 합니다.
    void OnCalculatePower(BattleCharacter owner, RuntimeSkill skill, ref int finalPower);

    // 3. 공격 적중 시점 (피해량 계산 직후)
    void OnSucceedAttack(BattleCharacter owner, BattleCharacter target, int damage);

    // 4. 턴 종료 시점 (상태 이상 데미지 정산 등)
    void OnTurnEnd(BattleCharacter owner);
}