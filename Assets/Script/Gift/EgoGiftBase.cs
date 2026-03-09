using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Modular Gift", menuName = "Gift/Modular Gift")]
public class EgoGiftBase : ScriptableObject
{
    public int    giftId;
    public string giftName;

    [Header("발동 조건")]
    public EffectCondition triggerCondition;

    [Header("기본 경로 (우선 판별)")]
    public List<GiftCondition> conditions = new List<GiftCondition>();
    public List<GiftEffect>    effects    = new List<GiftEffect>();

    [Header("대체 경로 (기본 실패 시 판별)")]
    public bool            useAlternative         = false;
    public List<GiftCondition> alternativeConditions = new List<GiftCondition>();
    public List<GiftEffect>    alternativeEffects    = new List<GiftEffect>();

    public virtual void OnEquip()
    {
        switch (triggerCondition)
        {
            case EffectCondition.OnHit:      BattleEventBus.OnHit      += HandleOnHit;      break;
            case EffectCondition.OnClashWin: BattleEventBus.OnClashWin += HandleOnClashWin; break;
            case EffectCondition.OnTurnEnd:  BattleEventBus.OnTurnEnd  += HandleOnTurnEnd;  break;
        }
    }

    public virtual void OnUnequip()
    {
        switch (triggerCondition)
        {
            case EffectCondition.OnHit:      BattleEventBus.OnHit      -= HandleOnHit;      break;
            case EffectCondition.OnClashWin: BattleEventBus.OnClashWin -= HandleOnClashWin; break;
            case EffectCondition.OnTurnEnd:  BattleEventBus.OnTurnEnd  -= HandleOnTurnEnd;  break;
        }
    }

    protected virtual void HandleOnHit(BattleCharacter attacker, BattleCharacter target, RuntimeSkill skill, int damage)
        => ExecuteModularLogic(attacker, target, skill, damage);

    protected virtual void HandleOnClashWin(BattleCharacter winner, BattleCharacter loser, int clashCount)
        => ExecuteModularLogic(winner, loser, null, 0);

    protected virtual void HandleOnTurnEnd(BattleCharacter unit)
        => ExecuteModularLogic(unit, unit, null, 0);

    private void ExecuteModularLogic(BattleCharacter actor, BattleCharacter target, RuntimeSkill skill, int damage)
    {
        if (AllConditionsMet(conditions, actor, target, skill, damage))
        {
            foreach (var effect in effects) effect.ApplyEffect(actor, target, skill);
            Debug.Log($"[Gift] 발동 (기본): {giftName}");
            return;
        }

        if (useAlternative && AllConditionsMet(alternativeConditions, actor, target, skill, damage))
        {
            foreach (var effect in alternativeEffects) effect.ApplyEffect(actor, target, skill);
            Debug.Log($"[Gift] 발동 (대체): {giftName}");
        }
    }

    private bool AllConditionsMet(List<GiftCondition> conds, BattleCharacter actor, BattleCharacter target, RuntimeSkill skill, int damage)
    {
        foreach (var cond in conds)
            if (!cond.IsMet(actor, target, skill, damage)) return false;
        return true;
    }
}
