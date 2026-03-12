using System.Collections.Generic;
using UnityEngine;

public class DashboardManager : MonoBehaviour
{
    [SerializeField] private BattleManager battleManager;
    [SerializeField] private GameObject dashboardSlotPrefab;
    [SerializeField] private Transform bottomPanel;

    private List<DashboardSlotUI> _slots = new List<DashboardSlotUI>();

    private void OnEnable()
    {
        if (battleManager != null)
        {
            battleManager.Events.OnBattleInit += HandleBattleInit;
            battleManager.Events.OnEnterWait += HandleEnterWait;

            // 캐릭터 교체 및 전투 리셋 시 스킬 덱 화면 갱신 구독
            battleManager.Events.OnCharacterSwapped += HandleCharacterSwapped;
            battleManager.Events.OnBattleReset += HandleEnterWait;
        }
    }

    private void OnDisable()
    {
        if (battleManager != null)
        {
            battleManager.Events.OnBattleInit -= HandleBattleInit;
            battleManager.Events.OnEnterWait -= HandleEnterWait;
            battleManager.Events.OnCharacterSwapped -= HandleCharacterSwapped;
            battleManager.Events.OnBattleReset -= HandleEnterWait;
        }
    }

    private void HandleCharacterSwapped(int index, bool isPlayerTeam, BattleCharacter newCharacter)
    {
        // 적군 교체는 하단 대시보드에 영향이 없으므로 무시
        if (!isPlayerTeam) return;

        if (index >= 0 && index < _slots.Count)
        {
            // 슬롯에 새로운 캐릭터 데이터 주입 후 갱신
            _slots[index].Setup(newCharacter);
        }
    }

    private void HandleBattleInit(List<BattleCharacter> players, List<BattleCharacter> enemies)
    {
        // 아군(PlayerTeam)만 하단 대시보드에 생성
        foreach (var player in players)
        {
            GameObject obj = Instantiate(dashboardSlotPrefab, bottomPanel);
            DashboardSlotUI slotUI = obj.GetComponent<DashboardSlotUI>();
            if (slotUI != null)
            {
                slotUI.Setup(player);
                _slots.Add(slotUI);
            }
        }
    }

    private void HandleEnterWait()
    {
        // 턴(Wait 상태)이 시작될 때마다 UI 스킬 덱을 새로고침
        foreach (var slot in _slots)
        {
            slot.UpdateSkills();
        }
    }
}
