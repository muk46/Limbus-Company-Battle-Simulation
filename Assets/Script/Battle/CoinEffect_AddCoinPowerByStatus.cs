using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Coin Effects/Add Coin Power By Status")]
public class CoinEffect_AddCoinPowerByStatus : BaseCoinEffect
{
    public StatusEffectType targetStatus = StatusEffectType.Bleed;
    public int divisor = 6;
    public int maxBonus = 2;

    public override void Execute(BattleCharacter attacker, BattleCharacter target, RuntimeSkill skill)
    {
        StatusEffect effect = target.ActiveEffects.Find(e => e.effectType == targetStatus);
        int potency = effect != null ? effect.potency : 0;
        int bonus = Mathf.FloorToInt(potency / divisor);
        bonus = Mathf.Min(bonus, maxBonus);

        if (bonus > 0)
            skill.CurrentCoinPower += bonus;
    }
}
