using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatusIconUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text potencyText;
    [SerializeField] private TMP_Text countText;

    // 상태이상별 색상 (아이콘 스프라이트가 없을 때 색상으로 구분)
    private static readonly Color[] StatusColors = {
        Color.white,        // None
        new Color(1f, 0.4f, 0.1f),  // Burn (주황)
        new Color(1f, 0.2f, 0.2f),  // Bleed (빨강)
        new Color(0.3f, 0.6f, 1f),  // Tremor (파랑)
        new Color(0.8f, 0.2f, 0.5f), // Rupture (자홍)
        new Color(0.4f, 0.3f, 0.7f), // Sinking (보라)
        new Color(1f, 0.9f, 0.3f),  // Poise (노랑)
        new Color(0.3f, 0.9f, 0.5f)  // Charge (초록)
    };

    private static readonly string[] StatusNames = {
        "", "화상", "출혈", "전율", "파열", "침잠", "호흡", "충전"
    };

    public void Set(StatusEffectType type, int potency, int count)
    {
        int idx = (int)type;

        if (iconImage != null)
            iconImage.color = Color.white;

        if (potencyText != null)
            potencyText.text = potency.ToString();

        if (countText != null)
            countText.text = count.ToString();

        gameObject.name = $"Status_{StatusNames[idx]}";
    }
}
