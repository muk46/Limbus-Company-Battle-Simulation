using UnityEngine;

[CreateAssetMenu(menuName = "Gift/Effects/Damage By Status Calculation")]
public class Effect_DamageByStatus : GiftEffect
{
    public StatusEffectType targetStatus;
    public int maxCountLimit = 5; // 기획서 기준 파열 횟수 최대 5 상한

    public override void ApplyEffect(BattleCharacter attacker, BattleCharacter target, RuntimeSkill skill)
    {
        if (target == null) return;

        StatusEffect effect = target.ActiveEffects.Find(e => e.effectType == targetStatus);
        if (effect != null && effect.potency > 0 && effect.count > 0)
        {
            // 상한선 적용
            int appliedCount = Mathf.Min(effect.count, maxCountLimit);
            int damage = effect.potency * appliedCount;

            target.TakeDamage(damage);
            Debug.Log($"[Effect] 비례 피해 발동: {target.OriginData.characterName}에게 {damage} 피해 ({targetStatus} 연산)");
        }
    }
}