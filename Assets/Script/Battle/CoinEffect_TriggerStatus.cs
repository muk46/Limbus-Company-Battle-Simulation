using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Coin Effects/Trigger Status")]
public class CoinEffect_TriggerStatus : BaseCoinEffect
{
    public StatusEffectType effectToTrigger;
    public int triggerTimes = 1;
    public int countToDecrease = 1;

    public override void Execute(BattleCharacter attacker, BattleCharacter target, RuntimeSkill skill)
    {
        StatusEffect effect = target.ActiveEffects.Find(e => e.effectType == effectToTrigger);
        if (effect != null && effect.potency > 0 && effect.count > 0)
        {
            int actualTriggers = 0;
            for (int i = 0; i < triggerTimes; i++)
            {
                if (effect.count > 0)
                {
                    target.TakeDamage(effect.potency);
                    effect.count--;
                    actualTriggers++;
                }
                else
                {
                    break;
                }
            }

            Debug.Log($"[Effect] {effectToTrigger} {actualTriggers}회 강제 발동! 총 {effect.potency * actualTriggers} 피해. 남은 횟수: {Mathf.Max(0, effect.count)}");
        }
    }
}
