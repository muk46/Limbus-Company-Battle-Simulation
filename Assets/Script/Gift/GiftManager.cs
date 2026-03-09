using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GiftManager : MonoBehaviour
{
    public static GiftManager Instance { get; private set; }

    [Header("기프트 데이터베이스")]
    [Tooltip("프로젝트에 생성된 모든 기프트 ScriptableObject를 인스펙터에서 이곳에 할당하십시오.")]
    public List<EgoGiftBase> allGiftsDatabase = new List<EgoGiftBase>();

    // 런타임에 현재 켜져 있는(활성화된) 기프트들을 추적하는 딕셔너리
    private Dictionary<int, EgoGiftBase> _activeGifts = new Dictionary<int, EgoGiftBase>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // 외부(UI 버튼 등)에서 호출할 핵심 토글 메서드
    public void ToggleGift(int giftId, bool turnOn)
    {
        if (turnOn) EquipGift(giftId);
        else UnequipGift(giftId);
    }

    private void EquipGift(int giftId)
    {
        // 이미 켜져 있는 기프트라면 무시
        if (_activeGifts.ContainsKey(giftId)) return;

        // 데이터베이스에서 해당 ID의 기프트 원본 검색
        EgoGiftBase giftToEquip = allGiftsDatabase.FirstOrDefault(g => g.giftId == giftId);

        if (giftToEquip != null)
        {
            // [핵심] 원본 훼손 방지를 위해 복제본 생성
            EgoGiftBase instance = Instantiate(giftToEquip);

            _activeGifts.Add(giftId, instance);

            // 기프트 내부의 이벤트 구독 로직 실행
            instance.OnEquip();
            Debug.Log($"[System] 기프트 ON: {instance.giftName}");
        }
        else
        {
            Debug.LogWarning($"[System] ID {giftId}에 해당하는 기프트를 데이터베이스에서 찾을 수 없습니다.");
        }
    }

    private void UnequipGift(int giftId)
    {
        // 딕셔너리에서 활성화된 기프트를 찾아 해제
        if (_activeGifts.TryGetValue(giftId, out EgoGiftBase activeGift))
        {
            // 이벤트 구독 해제
            activeGift.OnUnequip();
            _activeGifts.Remove(giftId);

            // 메모리에서 복제본 삭제
            Destroy(activeGift);
            Debug.Log($"[System] 기프트 OFF (ID: {giftId})");
        }
    }

    // 전투 종료 시 또는 훈련장 초기화 시 모든 기프트 강제 해제
    public void ClearAllGifts()
    {
        List<int> activeIds = _activeGifts.Keys.ToList();
        foreach (int id in activeIds)
        {
            UnequipGift(id);
        }
    }
}