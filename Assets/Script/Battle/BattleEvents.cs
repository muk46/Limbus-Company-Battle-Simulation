using System;
using System.Collections.Generic;

public class BattleEvents
{
    // 1. 전투 초기화 (왼쪽 캐릭터, 오른쪽 캐릭터 객체 전달)
    public Action<List<BattleCharacter>, List<BattleCharacter>> OnBattleInit;

    // 2. 대기 상태 진입 / 퇴장
    public Action OnEnterWait;
    public Action OnExitWait;

    // 3. 합 진행 (현재 라운드, 왼쪽 위력, 오른쪽 위력)
    public Action<int, int, int> OnClashRoundResolved;

    // 4. 합 종료 (결과 데이터 구조체 전달)
    public Action<ClashResult> OnClashFinished;

    // 5. 데미지 적용 (결과 데이터 구조체 전달)
    public Action<DamageResult> OnDamageApplied;

    // 6. 런타임 캐릭터 교체 (교체된 슬롯 인덱스, 아군 여부, 새 캐릭터 객체)
    public Action<int, bool, BattleCharacter> OnCharacterSwapped;

    // 7. 전투 초기화 (체력, 정신력, 스킬 덱 등 원상 복구)
    public Action OnBattleReset;
}