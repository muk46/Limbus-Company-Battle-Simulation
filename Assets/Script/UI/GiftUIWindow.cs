using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem; // 신규 입력 시스템 네임스페이스 추가

public class GiftUIWindow : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject windowPanel;      // 껐다 킬 전체 창 (배경 패널)
    [SerializeField] private Transform contentTransform;  // Scroll View의 Content 객체
    [SerializeField] private GameObject giftTogglePrefab; // GiftToggleUI가 붙은 프리팹

    private void Start()
    {
        InitializeGiftsUI();
        if (windowPanel != null)
        {
            windowPanel.SetActive(false); // 시작 시에는 창을 숨김
        }
    }

    private void Update()
    {
        // 신규 인풋 시스템(New Input System) 문법으로 키 입력 감지 교체
        if (Keyboard.current != null && Keyboard.current.gKey.wasPressedThisFrame)
        {
            if (windowPanel != null)
            {
                windowPanel.SetActive(!windowPanel.activeSelf);
            }
        }
    }

    private void InitializeGiftsUI()
    {
        if (GiftManager.Instance == null) return;

        List<EgoGiftBase> allGifts = GiftManager.Instance.allGiftsDatabase;

        foreach (var gift in allGifts)
        {
            if (gift == null) continue;

            GameObject obj = Instantiate(giftTogglePrefab, contentTransform);
            GiftToggleUI toggleUI = obj.GetComponent<GiftToggleUI>();

            if (toggleUI != null)
            {
                toggleUI.Setup(gift);
            }
        }
    }
}