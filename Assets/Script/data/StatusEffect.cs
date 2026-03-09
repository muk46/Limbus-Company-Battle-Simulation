// StatusEffect.cs
// 직전 코드에 있던 전역 StatusEffectType을 사용하도록 수정합니다.
public class StatusEffect
{
    public StatusEffectType effectType; // 수정됨
    public int potency;
    public int count;

    public StatusEffect(StatusEffectType type, int initialPotency, int initialCount)
    {
        effectType = type;
        potency = initialPotency;
        count = initialCount;
    }

    public void Add(int addPotency, int addCount)
    {
        potency += addPotency;
        count += addCount;
    }
}