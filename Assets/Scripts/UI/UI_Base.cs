using UnityEngine;
using UnityEngine.UI;
using TMPro;

public abstract class UI_Base : MonoBehaviour
{
    private DataBinder _dataBinder;

    private void Awake()
    {
        _dataBinder = new(gameObject);
        Init();
    }

    protected abstract void Init();

    public T Get<T>(string id) where T : Object => _dataBinder.Get<T>(id);
    public GameObject GetObject(string id) => _dataBinder.Get<GameObject>(id);
    public RectTransform GetRectTransform(string id) => _dataBinder.Get<RectTransform>(id);
    public Image GetImage(string id) => _dataBinder.Get<Image>(id);
    public RawImage GetRawImage(string id) => _dataBinder.Get<RawImage>(id);
    public TextMeshProUGUI GetText(string id) => _dataBinder.Get<TextMeshProUGUI>(id);
    public Button GetButton(string id) => _dataBinder.Get<Button>(id);
    public Toggle GetToggle(string id) => _dataBinder.Get<Toggle>(id);
    public Slider GetSlider(string id) => _dataBinder.Get<Slider>(id);
    public Scrollbar GetScrollbar(string id) => _dataBinder.Get<Scrollbar>(id);
    public ScrollRect GetScrollRect(string id) => _dataBinder.Get<ScrollRect>(id);
    public TMP_Dropdown GetDropdown(string id) => _dataBinder.Get<TMP_Dropdown>(id);
    public TMP_InputField GetInputField(string id) => _dataBinder.Get<TMP_InputField>(id);
}
