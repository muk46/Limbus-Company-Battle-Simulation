using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Coin Effects/Scale Status By Target")]
public class CoinEffect_ScaleStatusByTarget : BaseCoinEffect
{
    public StatusEffectType referenceEffect;
    public float divideBy = 3f;
    public StatusEffectType effectToApply;
    public int maxLimit = 5;

    public override void Execute(BattleCharacter attacker, BattleCharacter target, RuntimeSkill skill)
    {
        StatusEffect refEffect = target.ActiveEffects.Find(e => e.effectType == referenceEffect);
        int refPotency = refEffect != null ? refEffect.potency : 0;
        int calculatedPotency = Mathf.FloorToInt(refPotency / divideBy);
        calculatedPotency = Mathf.Min(calculatedPotency, maxLimit);

        if (calculatedPotency > 0)
        {
            target.AddStatusEffect(effectToApply, calculatedPotency, 0);
            Debug.Log($"[Effect] 비례 부여 발동: {target.OriginData.characterName}에게 {effectToApply} {calculatedPotency} 부여");
        }
    }
}
