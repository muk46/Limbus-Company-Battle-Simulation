// BattleEventManager.cs
using System;
using UnityEngine;

public class BattleEventManager : MonoBehaviour
{
    public static BattleEventManager Instance;

    // 기프트 및 시스템 발동 시점 정의
    public event Action OnBattleStart;
    public event Action<BattleCharacter, BattleCharacter> OnClashStart;

    // 수정됨: 합 횟수(clashCount) 전달용 int 매개변수 추가
    public event Action<BattleCharacter, BattleCharacter, int> OnClashWin;

    public event Action<BattleCharacter, BattleCharacter, RuntimeSkill, int> OnHit;
    public event Action<BattleCharacter> OnTurnEnd;
    public event Action<BattleCharacter> OnEnemyDeath;

    // 신규 추가: 사망한 캐릭터와 처치자를 함께 전달하는 이벤트
    public event Action<BattleCharacter, BattleCharacter> OnCharacterDeath;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void TriggerBattleStart()
    {
        OnBattleStart?.Invoke();
    }

    public void TriggerClashStart(BattleCharacter attacker, BattleCharacter defender)
    {
        OnClashStart?.Invoke(attacker, defender);
    }

    // 수정됨: 매개변수에 int clashCount 추가
    public void TriggerClashWin(BattleCharacter winner, BattleCharacter loser, int clashCount)
    {
        OnClashWin?.Invoke(winner, loser, clashCount);
    }

    public void TriggerOnHit(BattleCharacter attacker, BattleCharacter target, RuntimeSkill skill, int damage)
    {
        OnHit?.Invoke(attacker, target, skill, damage);
    }

    public void TriggerTurnEnd(BattleCharacter unit)
    {
        OnTurnEnd?.Invoke(unit);
    }

    public void TriggerEnemyDeath(BattleCharacter deadUnit)
    {
        OnEnemyDeath?.Invoke(deadUnit);
    }

    // 신규 추가: 사망 판정 시 처치자 식별용 트리거
    public void TriggerCharacterDeath(BattleCharacter deadUnit, BattleCharacter killer)
    {
        OnCharacterDeath?.Invoke(deadUnit, killer);
    }
}