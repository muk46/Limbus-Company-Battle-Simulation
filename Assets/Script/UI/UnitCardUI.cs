using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UnitCardUI : MonoBehaviour
{
    [Header("Data Reference")]
    private BattleCharacter _targetCharacter;

    [Header("UI Components")]
    [SerializeField] private TMP_Text speedText;
    [SerializeField] private Slider hpSlider;
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private TMP_Text spText;

    [Header("Status Effect Icons")]
    [SerializeField] private Transform statusIconContainer;
    [SerializeField] private GameObject statusIconPrefab;

    public void Setup(BattleCharacter character)
    {
        _targetCharacter = character;

        if (hpSlider != null)
        {
            hpSlider.minValue = 0;
            hpSlider.maxValue = _targetCharacter.OriginData.maxHealth;
        }

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

        if (speedText != null)
            speedText.text = _targetCharacter.CurrentSpeed.ToString();

        if (spText != null)
            spText.text = $"SP: {_targetCharacter.CurrentSanity}";

        UpdateStatusIcons();
    }

    private void UpdateStatusIcons()
    {
        if (statusIconContainer == null || statusIconPrefab == null) return;

        // 기존 아이콘 정리
        for (int i = statusIconContainer.childCount - 1; i >= 0; i--)
            Destroy(statusIconContainer.GetChild(i).gameObject);

        // 활성 상태이상 아이콘 생성
        foreach (var effect in _targetCharacter.ActiveEffects)
        {
            if (effect.count <= 0) continue;

            GameObject icon = Instantiate(statusIconPrefab, statusIconContainer);
            StatusIconUI iconUI = icon.GetComponent<StatusIconUI>();
            if (iconUI != null)
                iconUI.Set(effect.effectType, effect.potency, effect.count);
        }
    }
}
