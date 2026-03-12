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
        Debug.Log("[System] ���� ����(Execute) ������ ����: �ӵ� ������ �ൿ�� �����մϴ�.");

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
                Debug.Log($"[System] {currentAction.Attacker.OriginData.characterName}��(��) ��Ʈ���� ���·� �ൿ�� ��ҵǾ����ϴ�.");
                continue;
            }

            Debug.Log($"[System] {currentAction.Attacker.OriginData.characterName}�� ��! (�ӵ�: {currentAction.Attacker.CurrentSpeed})");

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
                    Debug.Log($"[System] {currentAction.Attacker.OriginData.characterName}�� {opponentAction.Attacker.OriginData.characterName}�� ��ȣ Ÿ���� ��(Clash) �߻�!");
                }
                else if (currentAction.Attacker.CurrentSpeed > opponentAction.Attacker.CurrentSpeed)
                {
                    isClash = true;
                    opponentAction.Target = currentAction.Attacker;
                    Debug.Log($"[System] �� ����ä�� ����: {currentAction.Attacker.OriginData.characterName}(�ӵ� {currentAction.Attacker.CurrentSpeed})�� {opponentAction.Attacker.OriginData.characterName}(�ӵ� {opponentAction.Attacker.CurrentSpeed})�� �ü��� ����ɴϴ�.");
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
                    Debug.Log($"[System] Ÿ��({currentAction.Target.OriginData.characterName})�� �̹� ����Ͽ� �ൿ�� ����մϴ�.");
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