using UnityEngine;
using UnityEngine.EventSystems;

public class SkillDragController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Line Rendering")]
    private LineRenderer _lineRenderer;
    private Camera _mainCamera;
    private Vector3 _startWorldPos;

    public BattleCharacter Attacker { get; private set; }
    public RuntimeSkill SkillToUse { get; private set; }

    public void Setup(BattleCharacter attacker, RuntimeSkill skill)
    {
        Attacker = attacker;
        SkillToUse = skill;
    }

    private void Start()
    {
        _mainCamera = Camera.main;
        _lineRenderer = gameObject.AddComponent<LineRenderer>();
        _lineRenderer.positionCount = 2;
        _lineRenderer.startWidth = 0.05f;
        _lineRenderer.endWidth = 0.05f;
        _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        _lineRenderer.startColor = Color.yellow;
        _lineRenderer.endColor = Color.red;
        _lineRenderer.enabled = false;
        _lineRenderer.sortingOrder = 100;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("[System] 드래그 시작됨!");

        // 캔버스가 Screen Space - Camera이므로 transform.position 자체가 월드 좌표입니다.
        _startWorldPos = transform.position;
        _startWorldPos.z = 0f; // 2D 씬의 기본 Z 평면으로 맞추어 다른 오브젝트에 가려지지 않게 함

        _lineRenderer.SetPosition(0, _startWorldPos);
        _lineRenderer.SetPosition(1, _startWorldPos);
        _lineRenderer.enabled = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 마우스 위치는 스크린 좌표이므로 월드 좌표로 변환이 필요합니다.
        Vector3 mouseScreenPos = eventData.position;
        mouseScreenPos.z = Mathf.Abs(_mainCamera.transform.position.z);

        Vector3 mouseWorldPos = _mainCamera.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0f; // 시작점과 동일하게 Z 평면 유지

        _lineRenderer.SetPosition(1, mouseWorldPos);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _lineRenderer.enabled = false;

        // 수정: Input.mousePosition 대신 eventData.position 사용
        Vector3 mouseScreenPos = eventData.position;
        mouseScreenPos.z = 10f;
        Vector2 mouseWorldPos = _mainCamera.ScreenToWorldPoint(mouseScreenPos);

        Collider2D hitCollider = Physics2D.OverlapPoint(mouseWorldPos);

        if (hitCollider != null)
        {
            TargetSlot targetSlot = hitCollider.GetComponent<TargetSlot>();

            if (targetSlot != null && targetSlot.Owner != null)
            {
                if (BattleManager.Instance != null)
                {
                    BattleManager.Instance.RegisterAction(Attacker, SkillToUse, targetSlot.Owner);
                }
            }
            else
            {
                Debug.Log("[System] 타겟팅 실패: 타겟 슬롯이 아닙니다.");
            }
        }
        else
        {
            Debug.Log("[System] 타겟팅 취소: 빈 공간에 드롭했습니다.");
        }
    }
}