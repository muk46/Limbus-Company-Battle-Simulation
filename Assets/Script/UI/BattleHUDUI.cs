using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BattleHUDUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private BattleManager battleManager;

    [Header("Top")]
    [SerializeField] private TMP_Text stateText;

    [Header("Left Unit (Player 1)")]
    [SerializeField] private TMP_Text leftNameText;
    [SerializeField] private Slider   leftHpSlider;
    [SerializeField] private TMP_Text leftHpText;
    [SerializeField] private TMP_Text leftCoinText;

    [Header("Right Unit (Enemy)")]
    [SerializeField] private TMP_Text rightNameText;
    [SerializeField] private Slider   rightHpSlider;
    [SerializeField] private TMP_Text rightHpText;
    [SerializeField] private TMP_Text rightCoinText;

    [Header("Center Clash")]
    [SerializeField] private TMP_Text roundText;
    [SerializeField] private TMP_Text powerText;
    [SerializeField] private TMP_Text winnerText;

    private void Awake()
    {
        if (battleManager == null)
            battleManager = FindFirstObjectByType<BattleManager>();
    }

    private void OnEnable()
    {
        BattleEventBus.OnBattleInit        += HandleBattleInit;
        BattleEventBus.OnEnterWait         += HandleEnterWait;
        BattleEventBus.OnClashRoundResolved += HandleClashRoundResolved;
        BattleEventBus.OnClashFinished     += HandleClashFinished;
        BattleEventBus.OnDamageApplied     += HandleDamageApplied;
        BattleEventBus.OnCharacterSwapped  += HandleCharacterSwapped;
        BattleEventBus.OnBattleReset       += HandleBattleReset;
    }

    private void OnDisable()
    {
        BattleEventBus.OnBattleInit        -= HandleBattleInit;
        BattleEventBus.OnEnterWait         -= HandleEnterWait;
        BattleEventBus.OnClashRoundResolved -= HandleClashRoundResolved;
        BattleEventBus.OnClashFinished     -= HandleClashFinished;
        BattleEventBus.OnDamageApplied     -= HandleDamageApplied;
        BattleEventBus.OnCharacterSwapped  -= HandleCharacterSwapped;
        BattleEventBus.OnBattleReset       -= HandleBattleReset;
    }

    private void HandleBattleInit(List<BattleCharacter> players, List<BattleCharacter> enemies)
    {
        if (players.Count == 0 || enemies.Count == 0) return;

        var left  = players[0];
        var right = enemies[0];

        if (leftNameText  != null) leftNameText.text  = left.OriginData.characterName;
        if (rightNameText != null) rightNameText.text = right.OriginData.characterName;

        if (leftHpSlider  != null) { leftHpSlider.minValue  = 0; leftHpSlider.maxValue  = left.OriginData.maxHealth; }
        if (rightHpSlider != null) { rightHpSlider.minValue = 0; rightHpSlider.maxValue = right.OriginData.maxHealth; }

        if (stateText != null) stateText.text = "State: Init";

        RefreshHpUI(left, right);
        ClearClashUI();
    }

    private void HandleEnterWait()
    {
        if (stateText != null) stateText.text = "State: Wait";
        ClearClashUI();
    }

    private void HandleClashRoundResolved(int round, int leftPower, int rightPower)
    {
        if (stateText  != null) stateText.text  = "State: Clash";
        if (roundText  != null) roundText.text  = $"Round: {round}";
        if (powerText  != null) powerText.text  = $"L {leftPower} : {rightPower} R";
    }

    private void HandleClashFinished(ClashResult result)
    {
        if (winnerText    != null) winnerText.text    = result.LeftWon ? "Winner: Left" : "Winner: Right";
        if (leftCoinText  != null) leftCoinText.text  = $"Coins: {result.LeftSkill.GetRemainingCoinCount()}";
        if (rightCoinText != null) rightCoinText.text = $"Coins: {result.RightSkill.GetRemainingCoinCount()}";
    }

    private void HandleDamageApplied(DamageResult result)
    {
        if (stateText != null) stateText.text = "State: Execute";
        RefreshFromManager();
    }

    private void HandleCharacterSwapped(int index, bool isPlayerTeam, BattleCharacter newCharacter)
    {
        if (index != 0) return;

        if (isPlayerTeam)
        {
            if (leftNameText  != null) leftNameText.text      = newCharacter.OriginData.characterName;
            if (leftHpSlider  != null) leftHpSlider.maxValue  = newCharacter.OriginData.maxHealth;
        }
        else
        {
            if (rightNameText != null) rightNameText.text     = newCharacter.OriginData.characterName;
            if (rightHpSlider != null) rightHpSlider.maxValue = newCharacter.OriginData.maxHealth;
        }

        RefreshFromManager();
    }

    private void HandleBattleReset() => RefreshFromManager();

    private void RefreshFromManager()
    {
        if (battleManager.PlayerTeam.Count > 0 && battleManager.EnemyTeam.Count > 0)
            RefreshHpUI(battleManager.PlayerTeam[0], battleManager.EnemyTeam[0]);
    }

    private void RefreshHpUI(BattleCharacter left, BattleCharacter right)
    {
        if (leftHpSlider  != null) leftHpSlider.value  = left.CurrentHealth;
        if (rightHpSlider != null) rightHpSlider.value = right.CurrentHealth;
        if (leftHpText    != null) leftHpText.text     = $"{left.CurrentHealth} / {left.OriginData.maxHealth}";
        if (rightHpText   != null) rightHpText.text    = $"{right.CurrentHealth} / {right.OriginData.maxHealth}";
    }

    private void ClearClashUI()
    {
        if (roundText     != null) roundText.text     = "Round: -";
        if (powerText     != null) powerText.text     = "L - : - R";
        if (winnerText    != null) winnerText.text    = "Winner: -";
        if (leftCoinText  != null) leftCoinText.text  = "Coins: -";
        if (rightCoinText != null) rightCoinText.text = "Coins: -";
    }
}
