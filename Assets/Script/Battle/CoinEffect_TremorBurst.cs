using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Coin Effects/Tremor Burst")]
public class CoinEffect_TremorBurst : BaseCoinEffect
{
    public override void Execute(BattleCharacter attacker, BattleCharacter target, RuntimeSkill skill)
    {
        StatusEffect tremor = target.ActiveEffects.Find(e => e.effectType == StatusEffectType.Tremor);
        if (tremor != null && tremor.potency > 0 && tremor.count > 0)
        {
            target.AddStagger(tremor.potency);
            tremor.count -= 1;
            Debug.Log($"[Effect] 진동 폭발 발동! {target.OriginData.characterName}의 흐트러짐 임계치가 {tremor.potency} 전진했습니다.");
        }
    }
}
