using UnityEngine;

public class UI_View : UI_Base
{
    [field: SerializeField]
    public UIType UIType { get; protected set; }

    [field: SerializeField]
    public bool IsValidForUISettings { get; private set; } = true;

    public Canvas Canvas { get; private set; }

    protected override void Init()
    {
        Canvas = GetComponent<Canvas>();
        Canvas.sortingOrder = (int)UIType;
    }
}
