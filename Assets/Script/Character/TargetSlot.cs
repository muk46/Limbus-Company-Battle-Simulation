using UnityEngine;

// 이 스크립트가 붙어있는 오브젝트는 마우스 타겟팅의 대상이 될 수 있습니다.
public class TargetSlot : MonoBehaviour
{
    public BattleCharacter Owner { get; private set; }

    // UI가 생성될 때 UnitCardUI로부터 주인을 전달받습니다.
    public void Initialize(BattleCharacter owner)
    {
        Owner = owner;
    }
}