using UnityEngine;

public abstract class GiftCondition : ScriptableObject
{
    // 조건이 만족되었는지 여부를 true/false로 반환
    public abstract bool IsMet(BattleCharacter attacker, BattleCharacter target, RuntimeSkill skill, int damage = 0);
}