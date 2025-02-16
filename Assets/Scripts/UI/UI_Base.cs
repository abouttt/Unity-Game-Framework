using UnityEngine;
using UnityEngine.UI;
using TMPro;

public abstract class UI_Base : MonoBehaviour
{
    private DataBinder _binder;

    private void Awake()
    {
        _binder = new(gameObject);
        Init();
    }

    protected abstract void Init();

    public T Get<T>(string id) where T : Object => _binder.Get<T>(id);
    public GameObject GetObject(string id) => _binder.Get<GameObject>(id);
    public RectTransform GetRectTransform(string id) => _binder.Get<RectTransform>(id);
    public Image GetImage(string id) => _binder.Get<Image>(id);
    public RawImage GetRawImage(string id) => _binder.Get<RawImage>(id);
    public TextMeshProUGUI GetText(string id) => _binder.Get<TextMeshProUGUI>(id);
    public Button GetButton(string id) => _binder.Get<Button>(id);
    public Toggle GetToggle(string id) => _binder.Get<Toggle>(id);
    public Slider GetSlider(string id) => _binder.Get<Slider>(id);
    public Scrollbar GetScrollbar(string id) => _binder.Get<Scrollbar>(id);
    public ScrollRect GetScrollRect(string id) => _binder.Get<ScrollRect>(id);
    public TMP_Dropdown GetDropdown(string id) => _binder.Get<TMP_Dropdown>(id);
    public TMP_InputField GetInputField(string id) => _binder.Get<TMP_InputField>(id);
}
