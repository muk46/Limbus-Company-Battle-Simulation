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
    [SerializeField] private Slider leftHpSlider;
    [SerializeField] private TMP_Text leftHpText;
    [SerializeField] private TMP_Text leftCoinText;

    [Header("Right Unit (Enemy)")]
    [SerializeField] private TMP_Text rightNameText;
    [SerializeField] private Slider rightHpSlider;
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
        if (battleManager == null) return;

        battleManager.Events.OnBattleInit += HandleBattleInit;
        battleManager.Events.OnEnterWait += HandleEnterWait;
        battleManager.Events.OnClashRoundResolved += HandleClashRoundResolved;
        battleManager.Events.OnClashFinished += HandleClashFinished;
        battleManager.Events.OnDamageApplied += HandleDamageApplied;

        // 런타임 캐릭터 교체 및 전투 리셋 이벤트 구독 추가
        battleManager.Events.OnCharacterSwapped += HandleCharacterSwapped;
        battleManager.Events.OnBattleReset += HandleBattleReset;
    }

    private void OnDisable()
    {
        if (battleManager == null) return;

        battleManager.Events.OnBattleInit -= HandleBattleInit;
        battleManager.Events.OnEnterWait -= HandleEnterWait;
        battleManager.Events.OnClashRoundResolved -= HandleClashRoundResolved;
        battleManager.Events.OnClashFinished -= HandleClashFinished;
        battleManager.Events.OnDamageApplied -= HandleDamageApplied;

        // 런타임 캐릭터 교체 및 전투 리셋 이벤트 구독 해제 추가
        battleManager.Events.OnCharacterSwapped -= HandleCharacterSwapped;
        battleManager.Events.OnBattleReset -= HandleBattleReset;
    }

    // 수정: 단일 객체가 아닌 리스트를 받아와서 0번 인덱스 캐릭터의 정보를 UI에 띄움
    private void HandleBattleInit(List<BattleCharacter> players, List<BattleCharacter> enemies)
    {
        if (players.Count == 0 || enemies.Count == 0) return;

        BattleCharacter leftChar = players[0];
        BattleCharacter rightChar = enemies[0];

        if (leftNameText != null) leftNameText.text = leftChar.OriginData.characterName;
        if (rightNameText != null) rightNameText.text = rightChar.OriginData.characterName;

        if (leftHpSlider != null)
        {
            leftHpSlider.minValue = 0;
            leftHpSlider.maxValue = leftChar.OriginData.maxHealth;
        }
        if (rightHpSlider != null)
        {
            rightHpSlider.minValue = 0;
            rightHpSlider.maxValue = rightChar.OriginData.maxHealth;
        }

        if (stateText != null) stateText.text = "State: Init";

        RefreshHpUI(leftChar, rightChar);
        ClearClashUI();
    }

    private void RefreshHpUI(BattleCharacter leftChar, BattleCharacter rightChar)
    {
        if (leftHpSlider != null) leftHpSlider.value = leftChar.CurrentHealth;
        if (rightHpSlider != null) rightHpSlider.value = rightChar.CurrentHealth;

        if (leftHpText != null) leftHpText.text = $"{leftChar.CurrentHealth} / {leftChar.OriginData.maxHealth}";
        if (rightHpText != null) rightHpText.text = $"{rightChar.CurrentHealth} / {rightChar.OriginData.maxHealth}";
    }

    private void ClearClashUI()
    {
        if (roundText != null) roundText.text = "Round: -";
        if (powerText != null) powerText.text = "L - : - R";
        if (winnerText != null) winnerText.text = "Winner: -";
        if (leftCoinText != null) leftCoinText.text = "Coins: -";
        if (rightCoinText != null) rightCoinText.text = "Coins: -";
    }

    private void HandleEnterWait()
    {
        if (stateText != null) stateText.text = "State: Wait";
        ClearClashUI();
    }

    private void HandleClashRoundResolved(int round, int leftPower, int rightPower)
    {
        if (stateText != null) stateText.text = "State: Clash";
        if (roundText != null) roundText.text = $"Round: {round}";
        if (powerText != null) powerText.text = $"L {leftPower} : {rightPower} R";
    }

    private void HandleClashFinished(ClashResult result)
    {
        if (winnerText != null) winnerText.text = result.LeftWon ? "Winner: Left" : "Winner: Right";

        // 속성 참조에서 메서드 호출로 변경
        if (leftCoinText != null) leftCoinText.text = $"Coins: {result.LeftSkill.GetRemainingCoinCount()}";
        if (rightCoinText != null) rightCoinText.text = $"Coins: {result.RightSkill.GetRemainingCoinCount()}";
    }

    // 수정: 데미지 갱신 시 매니저의 팀 리스트에서 0번 인덱스 캐릭터의 체력을 가져옴
    private void HandleDamageApplied(DamageResult result)
    {
        if (stateText != null) stateText.text = "State: Execute";

        if (battleManager.PlayerTeam.Count > 0 && battleManager.EnemyTeam.Count > 0)
        {
            RefreshHpUI(battleManager.PlayerTeam[0], battleManager.EnemyTeam[0]);
        }
    }

    // 추가: 캐릭터 교체 시 UI 텍스트 및 체력 바 갱신 함수
    private void HandleCharacterSwapped(int index, bool isPlayerTeam, BattleCharacter newCharacter)
    {
        // 1:1 전투를 가정하므로 0번 인덱스만 처리합니다.
        if (index == 0)
        {
            if (isPlayerTeam)
            {
                if (leftNameText != null) leftNameText.text = newCharacter.OriginData.characterName;
                if (leftHpSlider != null) leftHpSlider.maxValue = newCharacter.OriginData.maxHealth;
            }
            else
            {
                if (rightNameText != null) rightNameText.text = newCharacter.OriginData.characterName;
                if (rightHpSlider != null) rightHpSlider.maxValue = newCharacter.OriginData.maxHealth;
            }

            if (battleManager.PlayerTeam.Count > 0 && battleManager.EnemyTeam.Count > 0)
            {
                RefreshHpUI(battleManager.PlayerTeam[0], battleManager.EnemyTeam[0]);
            }
        }
    }

    // 추가: 리셋 시 HP 바 갱신 함수
    private void HandleBattleReset()
    {
        if (battleManager.PlayerTeam.Count > 0 && battleManager.EnemyTeam.Count > 0)
        {
            RefreshHpUI(battleManager.PlayerTeam[0], battleManager.EnemyTeam[0]);
        }
    }
}
