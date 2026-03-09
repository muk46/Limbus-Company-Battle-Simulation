using System.Collections.Generic;
using UnityEngine;

public class DashboardManager : MonoBehaviour
{
    [SerializeField] private GameObject  dashboardSlotPrefab;
    [SerializeField] private Transform   bottomPanel;

    private List<DashboardSlotUI> _slots = new List<DashboardSlotUI>();

    private void OnEnable()
    {
        BattleEventBus.OnBattleInit       += HandleBattleInit;
        BattleEventBus.OnEnterWait        += HandleEnterWait;
        BattleEventBus.OnCharacterSwapped += HandleCharacterSwapped;
        BattleEventBus.OnBattleReset      += HandleEnterWait;
    }

    private void OnDisable()
    {
        BattleEventBus.OnBattleInit       -= HandleBattleInit;
        BattleEventBus.OnEnterWait        -= HandleEnterWait;
        BattleEventBus.OnCharacterSwapped -= HandleCharacterSwapped;
        BattleEventBus.OnBattleReset      -= HandleEnterWait;
    }

    private void HandleBattleInit(List<BattleCharacter> players, List<BattleCharacter> enemies)
    {
        foreach (var player in players)
        {
            var obj   = Instantiate(dashboardSlotPrefab, bottomPanel);
            var slotUI = obj.GetComponent<DashboardSlotUI>();
            if (slotUI != null)
            {
                slotUI.Setup(player);
                _slots.Add(slotUI);
            }
        }
    }

    private void HandleEnterWait()
    {
        foreach (var slot in _slots) slot.UpdateSkills();
    }

    private void HandleCharacterSwapped(int index, bool isPlayerTeam, BattleCharacter newCharacter)
    {
        if (!isPlayerTeam) return;
        if (index >= 0 && index < _slots.Count)
            _slots[index].Setup(newCharacter);
    }
}
