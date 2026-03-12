using System.Text;
using TMPro;
using UnityEngine;

public class BattleLogUI : MonoBehaviour
{
    [SerializeField] private BattleManager battleManager;
    [SerializeField] private TMP_Text logText;
    [SerializeField] private int maxLines = 200;

    private readonly StringBuilder _sb = new StringBuilder(4096);
    private int _lineCount = 0;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            gameObject.SetActive(!gameObject.activeSelf);
        }
    }

    private void Awake()
    {
        if (battleManager == null)
            battleManager = FindFirstObjectByType<BattleManager>();

        if (logText == null)
            Debug.LogError("[BattleLogUI] LogText is not assigned.");
    }

    private void OnEnable()
    {
        if (battleManager == null) return;

        battleManager.Events.OnEnterWait += HandleEnterWait;
        battleManager.Events.OnExitWait += HandleExitWait;
        battleManager.Events.OnClashRoundResolved += HandleClashRoundResolved;
        battleManager.Events.OnClashFinished += HandleClashFinished;
        battleManager.Events.OnDamageApplied += HandleDamageApplied;
    }

    private void OnDisable()
    {
        if (battleManager == null) return;

        battleManager.Events.OnEnterWait -= HandleEnterWait;
        battleManager.Events.OnExitWait -= HandleExitWait;
        battleManager.Events.OnClashRoundResolved -= HandleClashRoundResolved;
        battleManager.Events.OnClashFinished -= HandleClashFinished;
        battleManager.Events.OnDamageApplied -= HandleDamageApplied;
    }

    private void HandleEnterWait()
    {
        AppendLine("Wait Enter");
    }

    private void HandleExitWait()
    {
        AppendLine("Wait Exit");
    }

    private void HandleClashRoundResolved(int round, int leftPower, int rightPower)
    {
        AppendLine($"Clash Round {round}: L={leftPower} R={rightPower}");
    }

    private void HandleClashFinished(ClashResult result)
    {
        string leftName = result.LeftSkill?.OriginData != null ? result.LeftSkill.OriginData.skillName : "Left";
        string rightName = result.RightSkill?.OriginData != null ? result.RightSkill.OriginData.skillName : "Right";
        string winner = result.LeftWon ? leftName : rightName;

        AppendLine($"Clash Finished: Winner={winner}");

        // 속성 참조에서 메서드 호출로 변경
        AppendLine($"Coins Left: L={result.LeftSkill.GetRemainingCoinCount()} R={result.RightSkill.GetRemainingCoinCount()}");
    }

    private void HandleDamageApplied(DamageResult result)
    {
        AppendLine($"Damage Amount: {result.finalDamage}");

        // 수정: 매니저의 팀 리스트에서 0번 인덱스 캐릭터의 체력을 가져옴
        if (battleManager.PlayerTeam.Count > 0 && battleManager.EnemyTeam.Count > 0)
        {
            AppendLine($"HP: L={battleManager.PlayerTeam[0].CurrentHealth} R={battleManager.EnemyTeam[0].CurrentHealth}");
        }
    }

    private void AppendLine(string line)
    {
        if (logText == null) return;

        _sb.AppendLine(line);
        _lineCount++;

        if (_lineCount > maxLines)
        {
            _sb.Clear();
            _lineCount = 0;
            _sb.AppendLine("[Log cleared: exceeded max lines]");
            _lineCount++;
        }

        logText.text = _sb.ToString();
    }
}
