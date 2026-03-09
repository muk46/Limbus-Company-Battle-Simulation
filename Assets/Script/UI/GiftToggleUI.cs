using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GiftToggleUI : MonoBehaviour
{
    [SerializeField] private Text giftNameText;
    [SerializeField] private Toggle toggleComponent;

    private int _targetGiftId;

    // [핵심] 컴파일러가 찾고 있던 Setup 함수입니다.
    public void Setup(EgoGiftBase giftData)
    {
        _targetGiftId = giftData.giftId;

        if (giftNameText != null)
            giftNameText.text = giftData.giftName;

        // 토글 값이 변경될 때 실행될 이벤트 연결
        toggleComponent.onValueChanged.RemoveAllListeners();
        toggleComponent.onValueChanged.AddListener(OnToggleValueChanged);
    }

    private void OnToggleValueChanged(bool isOn)
    {
        if (GiftManager.Instance != null)
        {
            GiftManager.Instance.ToggleGift(_targetGiftId, isOn);
        }
    }
}