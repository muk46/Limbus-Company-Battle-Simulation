using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Coin Effects/Amplify Damage By Debuffs")]
public class CoinEffect_AmplifyDamageByDebuffs : BaseCoinEffect
{
    public float bonusPerDebuff = 0.1f;
    public float maxBonus = 1.0f;

    public override void Execute(BattleCharacter attacker, BattleCharacter target, RuntimeSkill skill)
    {
        int debuffCount = 0;
        StatusEffectType[] debuffs = { StatusEffectType.Burn, StatusEffectType.Bleed, StatusEffectType.Tremor, StatusEffectType.Rupture, StatusEffectType.Sinking };

        foreach (var debuff in debuffs)
        {
            var effect = target.ActiveEffects.Find(e => e.effectType == debuff);
            if (effect != null && effect.count > 0 && effect.potency > 0) debuffCount++;
        }

        float totalBonus = Mathf.Min(debuffCount * bonusPerDebuff, maxBonus);
        skill.DamageMultiplierBonus += totalBonus;
    }
}
