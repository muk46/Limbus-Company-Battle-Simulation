using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class BattleExecuteState : IBattleState
{
    private BattleManager _manager;

    public BattleExecuteState(BattleManager manager)
    {
        _manager = manager;
    }

    public void Enter()
    {
        Debug.Log("[System] 실행 단계(Execute) 시작: 속도 순서로 행동을 처리합니다.");
        ResonanceCalculator.ApplyResonance(_manager.ActionQueue, _manager.PlayerTeam);
        _manager.RunRoutine(ExecuteActionsRoutine());
    }

    public void Execute() { }
    public void Exit()    { }

    private IEnumerator ExecuteActionsRoutine()
    {
        var sorted    = _manager.ActionQueue.OrderByDescending(a => a.Attacker.CurrentSpeed).ToList();
        var remaining = new List<BattleAction>(sorted);

        while (remaining.Count > 0)
        {
            var current = remaining[0];
            remaining.RemoveAt(0);
            _manager.ActionQueue.Remove(current);

            if (current.Attacker.CurrentHealth <= 0) continue;

            if (current.Attacker.StaggerLevel > 0)
            {
                Debug.Log($"[System] {current.Attacker.OriginData.characterName}은(는) 스태거 상태로 행동이 취소되었습니다.");
                continue;
            }

            Debug.Log($"[System] {current.Attacker.OriginData.characterName}의 턴! (속도: {current.Attacker.CurrentSpeed})");

            TriggerOnUseCoinEffects(current);

            // ── 클래시 감지 ──────────────────────────────
            var opponent = remaining.FirstOrDefault(a => a.Attacker == current.Target);
            bool isClash = ShouldClash(current, opponent);

            if (isClash)
            {
                TriggerOnUseCoinEffects(opponent);

                remaining.Remove(opponent);
                _manager.ActionQueue.Remove(opponent);

                if (current.Attacker.DeckSystem != null)  current.Attacker.DeckSystem.ConsumeSkill();
                if (opponent.Attacker.DeckSystem != null) opponent.Attacker.DeckSystem.ConsumeSkill();

                _manager.ChangeState(new BattleClashState(
                    _manager,
                    current.Attacker,  current.UsedSkill,
                    opponent.Attacker, opponent.UsedSkill));
                yield break;
            }

            // ── 일방적 공격 ──────────────────────────────
            if (current.Target.CurrentHealth <= 0)
            {
                Debug.Log($"[System] 타겟({current.Target.OriginData.characterName})이 이미 사망하여 행동을 건너뜁니다.");
                continue;
            }

            ClashCalculator.ResolveClash(current.Attacker, current.UsedSkill, current.Target);
            current.Attacker.DeckSystem?.ConsumeSkill();

            yield return new WaitForSeconds(1.0f);
        }

        // 턴 종료 상태이상 처리
        foreach (var c in _manager.PlayerTeam.Where(p => p.CurrentHealth > 0))
            c.ProcessTurnEndStatusEffects();
        foreach (var c in _manager.EnemyTeam.Where(e => e.CurrentHealth > 0))
            c.ProcessTurnEndStatusEffects();

        _manager.ActionQueue.Clear();
        _manager.ChangeState(new BattleWaitState(_manager));
    }

    /// <summary>두 행동이 클래시해야 하는지 판단합니다.</summary>
    private bool ShouldClash(BattleAction current, BattleAction opponent)
    {
        if (opponent == null) return false;
        if (opponent.Attacker.StaggerLevel > 0) return false;
        return opponent.Target == current.Attacker;
    }

    private void TriggerOnUseCoinEffects(BattleAction action)
    {
        if (action.UsedSkill.OriginData.coins == null) return;

        foreach (var coin in action.UsedSkill.OriginData.coins)
        {
            if (coin?.coinEffects == null) continue;
            foreach (var effect in coin.coinEffects)
            {
                if (effect == null) continue;
                if (effect.triggerCondition == EffectCondition.OnUse)
                    effect.Execute(action.Attacker, action.Target, action.UsedSkill);
            }
        }
    }
}
