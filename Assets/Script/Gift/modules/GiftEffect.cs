using UnityEngine;

// 모든 효과 블록의 부모 클래스
public abstract class GiftEffect : ScriptableObject
{
    // 수정됨: 자식 클래스들과 동일하게 RuntimeSkill 매개변수 추가
    public abstract void ApplyEffect(BattleCharacter attacker, BattleCharacter target, RuntimeSkill skill);
}