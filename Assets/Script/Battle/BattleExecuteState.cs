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
        Debug.Log("[System] 실행 단계(Execute) 상태로 진입: 속도 순서대로 행동을 실행합니다.");

        ClashCalculator.ResetCustomIndex();

        ResonanceCalculator.ApplyResonance(_manager.ActionQueue, _manager.PlayerTeam);

        _manager.RunRoutine(ExecuteActionsRoutine());
    }

    public void Execute() { }
    public void Exit() { }

    private IEnumerator ExecuteActionsRoutine()
    {
        var sortedActions = _manager.ActionQueue.OrderByDescending(action => action.Attacker.CurrentSpeed).ToList();
        List<BattleAction> remainingActions = new List<BattleAction>(sortedActions);

        while (remainingActions.Count > 0)
        {
            BattleAction currentAction = remainingActions[0];
            remainingActions.RemoveAt(0);
            _manager.ActionQueue.Remove(currentAction);

            if (currentAction.Attacker.CurrentHealth <= 0) continue;

            if (currentAction.Attacker.StaggerLevel > 0)
            {
                Debug.Log($"[System] {currentAction.Attacker.OriginData.characterName}은(는) 흐트러짐 상태로 행동이 취소되었습니다.");
                continue;
            }

            Debug.Log($"[System] {currentAction.Attacker.OriginData.characterName}의 턴! (속도: {currentAction.Attacker.CurrentSpeed})");

            if (currentAction.UsedSkill.OriginData.coins != null)
            {
                foreach (var coin in currentAction.UsedSkill.OriginData.coins)
                {
                    if (coin.coinEffects == null) continue;
                    foreach (var effect in coin.coinEffects)
                    {
                        if (effect.triggerCondition == EffectCondition.OnUse)
                        {
                            effect.Execute(currentAction.Attacker, currentAction.Target, currentAction.UsedSkill);
                        }
                    }
                }
            }

            BattleAction opponentAction = remainingActions.FirstOrDefault(a => a.Attacker == currentAction.Target);
            bool isClash = false;

            if (opponentAction != null && opponentAction.Attacker.StaggerLevel == 0)
            {
                if (opponentAction.Target == currentAction.Attacker)
                {
                    isClash = true;
                    Debug.Log($"[System] {currentAction.Attacker.OriginData.characterName}와 {opponentAction.Attacker.OriginData.characterName}의 상호 타겟으로 합(Clash) 발생!");
                }
                else if (currentAction.Attacker.CurrentSpeed > opponentAction.Attacker.CurrentSpeed)
                {
                    isClash = true;
                    opponentAction.Target = currentAction.Attacker;
                    Debug.Log($"[System] 속도 우위에 의한 강제 합: {currentAction.Attacker.OriginData.characterName}(속도 {currentAction.Attacker.CurrentSpeed})이 {opponentAction.Attacker.OriginData.characterName}(속도 {opponentAction.Attacker.CurrentSpeed})의 타겟을 가로챘습니다.");
                }
            }

            if (isClash)
            {
                remainingActions.Remove(opponentAction);
                _manager.ActionQueue.Remove(opponentAction);

                if (opponentAction.UsedSkill.OriginData.coins != null)
                {
                    foreach (var coin in opponentAction.UsedSkill.OriginData.coins)
                    {
                        if (coin.coinEffects == null) continue;
                        foreach (var effect in coin.coinEffects)
                        {
                            if (effect.triggerCondition == EffectCondition.OnUse)
                            {
                                effect.Execute(opponentAction.Attacker, opponentAction.Target, opponentAction.UsedSkill);
                            }
                        }
                    }
                }

                if (currentAction.Attacker.DeckSystem != null) currentAction.Attacker.DeckSystem.ConsumeSkill();
                if (opponentAction.Attacker.DeckSystem != null) opponentAction.Attacker.DeckSystem.ConsumeSkill();

                _manager.ChangeState(new BattleClashState(_manager, currentAction.Attacker, currentAction.UsedSkill, opponentAction.Attacker, opponentAction.UsedSkill));
                yield break;
            }
            else
            {
                if (currentAction.Target.CurrentHealth <= 0)
                {
                    Debug.Log($"[System] 타겟({currentAction.Target.OriginData.characterName})이 이미 사망하여 행동을 건너뜁니다.");
                    continue;
                }

                ClashCalculator.ResolveClash(currentAction.Attacker, currentAction.UsedSkill, currentAction.Target);

                if (currentAction.Attacker.DeckSystem != null)
                {
                    currentAction.Attacker.DeckSystem.ConsumeSkill();
                }

                yield return new WaitForSeconds(1.0f);
            }
        }

        foreach (var chara in _manager.PlayerTeam.Where(p => p.CurrentHealth > 0)) chara.ProcessTurnEndStatusEffects();
        foreach (var chara in _manager.EnemyTeam.Where(e => e.CurrentHealth > 0)) chara.ProcessTurnEndStatusEffects();

        _manager.ActionQueue.Clear();
        _manager.ChangeState(new BattleWaitState(_manager));
    }
}