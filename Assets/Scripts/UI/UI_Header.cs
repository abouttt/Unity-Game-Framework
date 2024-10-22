using UnityEngine;
using UnityEngine.EventSystems;

public class UI_Header : MonoBehaviour, IBeginDragHandler, IDragHandler
{
    [field: SerializeField]
    public bool CanDrag { get; set; } = true;

    [SerializeField]
    private RectTransform _target;  // 헤더를 잡고 움직이면 따라서 UI들도 움직이게 하기위한 중심 타겟.

    private Vector2 _beginPoint;    // 헤더를 잡은 시작 위치.
    private Vector2 _moveBegin;     // 잡고 움직인 시작 위치.

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!CanDrag)
        {
            eventData.pointerDrag = null;
            return;
        }

        _beginPoint = _target.anchoredPosition;
        _moveBegin = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        _target.anchoredPosition = _beginPoint + (eventData.position - _moveBegin);
    }
}
