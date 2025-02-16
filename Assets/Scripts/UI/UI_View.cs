using UnityEngine;

public abstract class UI_View : UI_Base
{
    [field: SerializeField]
    public UIType UIType { get; protected set; }

    [field: SerializeField]
    public bool IsValidForUISettings { get; private set; } = true;

    public Canvas Canvas => _canvas;

    private Canvas _canvas;

    protected override void Init()
    {
        _canvas = GetComponent<Canvas>();
    }
}
