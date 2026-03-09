using UnityEngine;

[CreateAssetMenu(menuName = "Gift/Effects/Deal Damage")]
public class Effect_DealDamage : GiftEffect
{
    [Header("피해 대상")]
    public bool applyToTarget = true; // true: 피격자/상대방, false: 공격자/자신

    [Header("피해량 설정")]
    public int fixedDamageAmount = 5;

    public override void ApplyEffect(BattleCharacter attacker, BattleCharacter target, RuntimeSkill skill)
    {
        BattleCharacter finalTarget = applyToTarget ? target : attacker;
        if (finalTarget == null) return;

        finalTarget.TakeDamage(fixedDamageAmount);
        Debug.Log($"[Effect] {finalTarget.OriginData.characterName}에게 {fixedDamageAmount}의 고정 피해를 입혔습니다.");
    }
}