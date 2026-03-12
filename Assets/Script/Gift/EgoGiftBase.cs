// EgoGiftBase.cs
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Modular Gift", menuName = "Gift/Modular Gift")]
public class EgoGiftBase : ScriptableObject
{
    public int giftId;
    public string giftName;

    [Header("발동 시점")]
    public EffectCondition triggerCondition;

    // --- Path A: 강화/메인 효과 ---
    [Header("기본 모듈 (우선 판별)")]
    public List<GiftCondition> conditions = new List<GiftCondition>();
    public List<GiftEffect> effects = new List<GiftEffect>();

    // --- Path B: 기본/대체 효과 ---
    [Header("대체 모듈 (기본 조건 미충족 시 판별)")]
    public bool useAlternative = false;
    public List<GiftCondition> alternativeConditions = new List<GiftCondition>();
    public List<GiftEffect> alternativeEffects = new List<GiftEffect>();

    public virtual void OnEquip()
    {
        if (triggerCondition == EffectCondition.OnHit)
            BattleEventManager.Instance.OnHit += HandleOnHit;
        else if (triggerCondition == EffectCondition.OnClashWin)
            BattleEventManager.Instance.OnClashWin += HandleOnClashWin;
        else if (triggerCondition == EffectCondition.OnTurnEnd)
            BattleEventManager.Instance.OnTurnEnd += HandleOnTurnEnd;
    }

    public virtual void OnUnequip()
    {
        if (triggerCondition == EffectCondition.OnHit)
            BattleEventManager.Instance.OnHit -= HandleOnHit;
        else if (triggerCondition == EffectCondition.OnClashWin)
            BattleEventManager.Instance.OnClashWin -= HandleOnClashWin;
        else if (triggerCondition == EffectCondition.OnTurnEnd)
            BattleEventManager.Instance.OnTurnEnd -= HandleOnTurnEnd;
    }

    protected virtual void HandleOnHit(BattleCharacter attacker, BattleCharacter target, RuntimeSkill skill, int damage)
    {
        ExecuteModularLogic(attacker, target, skill, damage);
    }

    // [수정됨] 매개변수에 int clashCount 추가
    protected virtual void HandleOnClashWin(BattleCharacter winner, BattleCharacter loser, int clashCount)
    {
        ExecuteModularLogic(winner, loser, null, 0);
    }

    protected virtual void HandleOnTurnEnd(BattleCharacter unit)
    {
        ExecuteModularLogic(unit, unit, null, 0);
    }

    private void ExecuteModularLogic(BattleCharacter actor, BattleCharacter target, RuntimeSkill skill, int damage)
    {
        bool primaryConditionsMet = true;

        // 1. 경로 A (우선 판별) 평가
        foreach (var condition in conditions)
        {
            if (!condition.IsMet(actor, target, skill, damage))
            {
                primaryConditionsMet = false;
                break;
            }
        }

        if (primaryConditionsMet)
        {
            // 경로 A 성공: 효과 발동 후 종료
            foreach (var effect in effects)
            {
                effect.ApplyEffect(actor, target, skill);
            }
            Debug.Log($"[System] 기프트 (강화) 발동: {giftName}");
            return;
        }

        // 2. 경로 A 실패, 대체 경로 사용 체크 시 평가
        if (useAlternative)
        {
            bool altConditionsMet = true;
            foreach (var condition in alternativeConditions)
            {
                if (!condition.IsMet(actor, target, skill, damage))
                {
                    altConditionsMet = false;
                    break;
                }
            }

            if (altConditionsMet)
            {
                // 경로 B 성공
                foreach (var effect in alternativeEffects)
                {
                    effect.ApplyEffect(actor, target, skill);
                }
                Debug.Log($"[System] 기프트 (기본) 발동: {giftName}");
            }
        }
    }
}
