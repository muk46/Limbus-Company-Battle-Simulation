using System.Text;
using TMPro;
using UnityEngine;

public class BattleLogUI : MonoBehaviour
{
    [SerializeField] private BattleManager battleManager;
    [SerializeField] private TMP_Text      logText;
    [SerializeField] private int           maxLines = 200;

    private readonly StringBuilder _sb = new StringBuilder(4096);
    private int _lineCount = 0;

    private void Awake()
    {
        if (battleManager == null)
            battleManager = FindFirstObjectByType<BattleManager>();

        if (logText == null)
            Debug.LogError("[BattleLogUI] LogText is not assigned.");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
            gameObject.SetActive(!gameObject.activeSelf);
    }

    private void OnEnable()
    {
        BattleEventBus.OnEnterWait          += HandleEnterWait;
        BattleEventBus.OnExitWait           += HandleExitWait;
        BattleEventBus.OnClashRoundResolved += HandleClashRoundResolved;
        BattleEventBus.OnClashFinished      += HandleClashFinished;
        BattleEventBus.OnDamageApplied      += HandleDamageApplied;
    }

    private void OnDisable()
    {
        BattleEventBus.OnEnterWait          -= HandleEnterWait;
        BattleEventBus.OnExitWait           -= HandleExitWait;
        BattleEventBus.OnClashRoundResolved -= HandleClashRoundResolved;
        BattleEventBus.OnClashFinished      -= HandleClashFinished;
        BattleEventBus.OnDamageApplied      -= HandleDamageApplied;
    }

    private void HandleEnterWait() => AppendLine("Wait Enter");
    private void HandleExitWait()  => AppendLine("Wait Exit");

    private void HandleClashRoundResolved(int round, int leftPower, int rightPower)
        => AppendLine($"Clash Round {round}: L={leftPower} R={rightPower}");

    private void HandleClashFinished(ClashResult result)
    {
        string left   = result.LeftSkill?.OriginData?.skillName  ?? "Left";
        string right  = result.RightSkill?.OriginData?.skillName ?? "Right";
        string winner = result.LeftWon ? left : right;
        AppendLine($"Clash Finished: Winner={winner}");
        AppendLine($"Coins Left: L={result.LeftSkill.GetRemainingCoinCount()} R={result.RightSkill.GetRemainingCoinCount()}");
    }

    private void HandleDamageApplied(DamageResult result)
    {
        AppendLine($"Damage Amount: {result.finalDamage}");
        if (battleManager.PlayerTeam.Count > 0 && battleManager.EnemyTeam.Count > 0)
            AppendLine($"HP: L={battleManager.PlayerTeam[0].CurrentHealth} R={battleManager.EnemyTeam[0].CurrentHealth}");
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
