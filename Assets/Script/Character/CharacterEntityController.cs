using UnityEngine;

public class CharacterEntityController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;

    // 추가: 내부에 부착된 개인 UI 상태창 연결용 변수
    [SerializeField] private UnitCardUI myStatusUI;

    public BattleCharacter Model { get; private set; }

    public void Initialize(BattleCharacter characterData)
    {
        Model = characterData;

        // [추가] 모델의 데이터가 변할 때 UI를 갱신하도록 이벤트 구독
        Model.OnUIUpdateRequest += UpdateUI;

        if (spriteRenderer != null && Model.OriginData.portrait != null)
        {
            spriteRenderer.sprite = Model.OriginData.portrait;
        }

        if (myStatusUI != null)
        {
            myStatusUI.Setup(Model);
        }

        Debug.Log($"[System] {Model.OriginData.characterName}의 월드 엔티티가 생성되었습니다.");
    }

    // [추가] 오브젝트 파괴 시 메모리 누수를 방지하기 위한 구독 해제
    private void OnDestroy()
    {
        if (Model != null)
        {
            Model.OnUIUpdateRequest -= UpdateUI;
        }
    }
    public void UpdateUI()
    {
        if (myStatusUI != null)
        {
            myStatusUI.UpdateUI();
        }
    }
    private LineRenderer _targetingLine;

    public void ShowTargetingLine(Transform targetTransform)
    {
        if (_targetingLine == null)
        {
            _targetingLine = gameObject.AddComponent<LineRenderer>();
            _targetingLine.positionCount = 2;
            _targetingLine.startWidth = 0.02f; // 선의 두께 (시작)
            _targetingLine.endWidth = 0.08f;   // 선의 두께 (끝 - 화살표 느낌)
            _targetingLine.material = new Material(Shader.Find("Sprites/Default"));
            _targetingLine.startColor = Color.red;
            _targetingLine.endColor = Color.red;
            _targetingLine.sortingOrder = 50; // UI보다 뒤, 캐릭터보다 앞에 그려지도록 정렬
        }

        _targetingLine.enabled = true;
        // 내 위치에서 타겟 위치로 선 그리기
        _targetingLine.SetPosition(0, transform.position);
        _targetingLine.SetPosition(1, targetTransform.position);
    }

    public void HideTargetingLine()
    {
        if (_targetingLine != null)
        {
            _targetingLine.enabled = false;
        }
    }
}
