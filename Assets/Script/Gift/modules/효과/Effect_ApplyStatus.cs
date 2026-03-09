using UnityEngine;

[CreateAssetMenu(menuName = "Gift/Effects/Apply Status")]
public class Effect_ApplyStatus : GiftEffect
{
    [Header("부여 대상")]
    public bool applyToTarget = true; // true: 피격자/상대방, false: 공격자/자신

    [Header("상태이상 설정")]
    public StatusEffectType effectType;
    public int potencyToAdd;
    public int countToAdd;

    public override void ApplyEffect(BattleCharacter attacker, BattleCharacter target, RuntimeSkill skill)
    {
        BattleCharacter finalTarget = applyToTarget ? target : attacker;
        if (finalTarget == null) return;

        finalTarget.AddStatusEffect(effectType, potencyToAdd, countToAdd);
        Debug.Log($"[Effect] {finalTarget.OriginData.characterName}에게 {effectType}(위력:{potencyToAdd}, 횟수:{countToAdd}) 부여됨.");
    }
}