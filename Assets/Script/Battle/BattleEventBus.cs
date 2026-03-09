using System;
using System.Collections.Generic;

/// <summary>
/// 배틀 내 모든 이벤트를 중앙에서 관리하는 정적 버스.
/// BattleEvents(UI용)와 BattleEventManager(게임플레이용)를 단일 진입점으로 통합.
/// </summary>
public static class BattleEventBus
{
    // ── 배틀 생명주기 ──────────────────────────────────────
    public static event Action<List<BattleCharacter>, List<BattleCharacter>> OnBattleInit;
    public static event Action OnBattleStart;
    public static event Action OnBattleReset;

    // ── Wait 상태 ─────────────────────────────────────────
    public static event Action OnEnterWait;
    public static event Action OnExitWait;

    // ── 클래시 ────────────────────────────────────────────
    public static event Action<int, int, int> OnClashRoundResolved;
    public static event Action<ClashResult> OnClashFinished;
    public static event Action<BattleCharacter, BattleCharacter, int> OnClashWin;

    // ── 타격 / 데미지 ─────────────────────────────────────
    public static event Action<BattleCharacter, BattleCharacter, RuntimeSkill, int> OnHit;
    public static event Action<DamageResult> OnDamageApplied;

    // ── 캐릭터 ────────────────────────────────────────────
    public static event Action<int, bool, BattleCharacter> OnCharacterSwapped;
    public static event Action<BattleCharacter, BattleCharacter> OnCharacterDeath;
    public static event Action<BattleCharacter> OnTurnEnd;

    // ── Raise 헬퍼 ────────────────────────────────────────
    public static void Raise_OnBattleInit(List<BattleCharacter> players, List<BattleCharacter> enemies)
        => OnBattleInit?.Invoke(players, enemies);

    public static void Raise_OnBattleStart()
        => OnBattleStart?.Invoke();

    public static void Raise_OnBattleReset()
        => OnBattleReset?.Invoke();

    public static void Raise_OnEnterWait()
        => OnEnterWait?.Invoke();

    public static void Raise_OnExitWait()
        => OnExitWait?.Invoke();

    public static void Raise_OnClashRoundResolved(int round, int leftPower, int rightPower)
        => OnClashRoundResolved?.Invoke(round, leftPower, rightPower);

    public static void Raise_OnClashFinished(ClashResult result)
        => OnClashFinished?.Invoke(result);

    public static void Raise_OnClashWin(BattleCharacter winner, BattleCharacter loser, int clashCount)
        => OnClashWin?.Invoke(winner, loser, clashCount);

    public static void Raise_OnHit(BattleCharacter attacker, BattleCharacter target, RuntimeSkill skill, int damage)
        => OnHit?.Invoke(attacker, target, skill, damage);

    public static void Raise_OnDamageApplied(DamageResult result)
        => OnDamageApplied?.Invoke(result);

    public static void Raise_OnCharacterSwapped(int index, bool isPlayerTeam, BattleCharacter newCharacter)
        => OnCharacterSwapped?.Invoke(index, isPlayerTeam, newCharacter);

    public static void Raise_OnCharacterDeath(BattleCharacter dead, BattleCharacter killer)
        => OnCharacterDeath?.Invoke(dead, killer);

    public static void Raise_OnTurnEnd(BattleCharacter unit)
        => OnTurnEnd?.Invoke(unit);

    /// <summary>씬/배틀 초기화 시 호출. 정적 이벤트 누수 방지.</summary>
    public static void ClearAll()
    {
        OnBattleInit       = null;
        OnBattleStart      = null;
        OnBattleReset      = null;
        OnEnterWait        = null;
        OnExitWait         = null;
        OnClashRoundResolved = null;
        OnClashFinished    = null;
        OnClashWin         = null;
        OnHit              = null;
        OnDamageApplied    = null;
        OnCharacterSwapped = null;
        OnCharacterDeath   = null;
        OnTurnEnd          = null;
    }
}
