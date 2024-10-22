using UnityEngine;

public class UI_View : MonoBehaviour
{
    [field: SerializeField]
    public UIType UIType { get; protected set; }

    public Canvas Canvas => _canvas;

    private Canvas _canvas;

    protected virtual void Awake()
    {
        if (TryGetComponent(out _canvas))
        {
            _canvas.sortingOrder = (int)UIType;
        }
        else
        {
            Debug.LogWarning($"{name} ui view object does not have a Canvas component.");
        }
    }
}
