using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitCardUI : MonoBehaviour
{
    [Header("Data Reference")]
    private BattleCharacter _targetCharacter;

    [Header("UI Components")]
    [SerializeField] private TMP_Text speedText;
    [SerializeField] private Slider hpSlider;
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private TMP_Text spText; // 정신력(Sanity) 텍스트 추가

    public void Setup(BattleCharacter character)
    {
        _targetCharacter = character;

        if (hpSlider != null)
        {
            hpSlider.minValue = 0;
            hpSlider.maxValue = _targetCharacter.OriginData.maxHealth;
        }

        // 추가: 내 게임 오브젝트에 TargetSlot이 있다면 데이터 연결
        TargetSlot targetSlot = GetComponent<TargetSlot>();
        if (targetSlot != null)
        {
            targetSlot.Initialize(_targetCharacter);
        }

        UpdateUI();
    }

    public void UpdateUI()
    {
        if (_targetCharacter == null) return;

        if (hpSlider != null) hpSlider.value = _targetCharacter.CurrentHealth;
        if (hpText != null) hpText.text = $"{_targetCharacter.CurrentHealth}/{_targetCharacter.OriginData.maxHealth}";

        // 수정: min - max 범위 텍스트 대신, 결정된 CurrentSpeed 하나만 출력합니다.
        if (speedText != null)
            speedText.text = _targetCharacter.CurrentSpeed.ToString();

        if (spText != null)
        {
            spText.text = $"SP: {_targetCharacter.CurrentSanity}";
        }
    }
}