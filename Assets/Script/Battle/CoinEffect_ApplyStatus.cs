using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Coin Effects/Apply Status")]
public class CoinEffect_ApplyStatus : BaseCoinEffect
{
    public StatusEffectType effectType;
    public int potency;
    public int count;

    public override void Execute(BattleCharacter attacker, BattleCharacter target, RuntimeSkill skill)
    {
        target.AddStatusEffect(effectType, potency, count);
    }
}
