using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_Popup : UI_View, IPointerDownHandler
{
    public event Action Focused;

    [field: SerializeField]
    public bool IsHelper { get; private set; }

    [field: SerializeField]
    public bool IsSelfish { get; private set; }

    [field: SerializeField]
    public bool IgnoreSelfish { get; private set; }

    [field: SerializeField]
    public RectTransform Body { get; private set; }

    [field: SerializeField]
    public Vector3 DefaultPosition { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        UIType = UIType.Popup;

        if (Body == null)
        {
            Body = transform.GetChild(0).GetComponent<RectTransform>();
        }

        Body.anchoredPosition = DefaultPosition;
    }

    protected virtual void Start()
    {
        gameObject.SetActive(false);
    }

    protected virtual void OnEnable()
    {
        InputManager.CursorLocked = false;
    }

    protected virtual void OnDisable()
    {
        if (UIManager.ActivePopupCount == 0)
        {
            InputManager.CursorLocked = true;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Focused?.Invoke();
    }
}
