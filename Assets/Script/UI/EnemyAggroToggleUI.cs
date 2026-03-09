using UnityEngine;
using UnityEngine.UI;

public class EnemyAggroToggleUI : MonoBehaviour
{
    private Toggle _toggle;

    private void Awake()
    {
        _toggle = GetComponent<Toggle>();
        if (_toggle != null)
        {
            _toggle.onValueChanged.AddListener(OnAggroToggled);
        }
    }

    private void Start()
    {
        // 씬 시작 시 BattleManager의 기본 상태와 UI 체크박스 상태를 동기화
        if (BattleManager.Instance != null && _toggle != null)
        {
            _toggle.isOn = BattleManager.Instance.isEnemyAggressive;
        }
    }

    private void OnAggroToggled(bool isOn)
    {
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.isEnemyAggressive = isOn;
            Debug.Log(isOn ? "[System] 훈련장 모드 변경: 적군 공격 활성화 (합 테스트)" : "[System] 훈련장 모드 변경: 적군 샌드백 모드 (일방 공격 테스트)");

            // [추가] 토글 즉시 적군의 행동을 다시 계산하여 선을 그리거나 지웁니다.
            BattleManager.Instance.GenerateEnemyActions();
        }
    }
}