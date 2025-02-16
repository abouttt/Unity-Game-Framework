using System;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviourSingleton<UIManager>
{
    public static int Count => Instance._objects.Count;
    public static int ActivePopupCount => Instance._activePopups.Count;
    public static bool IsActiveHelperPopup => Instance._helperPopup != null;
    public static bool IsActiveSelfishPopup => Instance._selfishPopup != null;

    private readonly Dictionary<UIType, Transform> _roots = new();
    private readonly Dictionary<Type, UI_View> _objects = new();
    private readonly LinkedList<UI_Popup> _activePopups = new();
    private UI_Popup _helperPopup;
    private UI_Popup _selfishPopup;

    protected override void Init()
    {
        base.Init();

        foreach (UIType type in Enum.GetValues(typeof(UIType)))
        {
            var root = new GameObject($"{type}_Root").transform;
            root.SetParent(transform);
            _roots.Add(type, root);
        }
    }

    protected override void Dispose()
    {
        base.Dispose();
        Clear();
    }

    public static void Register<T>(T ui) where T : UI_View
    {
        var instance = Instance;

        if (!instance._objects.TryGetValue(typeof(T), out _))
        {
            if (ui.UIType == UIType.Popup)
            {
                instance.RegisterPopup(ui as UI_Popup);
            }

            ui.transform.SetParent(instance._roots[ui.UIType]);
            instance._objects.Add(typeof(T), ui);
        }
        else
        {
            Debug.LogWarning($"[UIManager.Register] {typeof(T)} is already registered.");
        }
    }

    public static void Unregister<T>(bool destory = false) where T : UI_View
    {
        var instance = Instance;

        if (instance._objects.TryGetValue(typeof(T), out var ui))
        {
            if (ui.UIType == UIType.Popup)
            {
                instance.UnregisterPopup(ui as UI_Popup);
            }

            ui.transform.SetParent(null);
            instance._objects.Remove(typeof(T));

            if (destory)
            {
                Destroy(ui.gameObject);
            }
        }
        else
        {
            Debug.LogWarning($"[UIManager.Unregister] {typeof(T)} is not registered.");
        }
    }

    public static T Show<T>() where T : UI_View
    {
        var instance = Instance;

        if (instance._objects.TryGetValue(typeof(T), out var ui))
        {
            if (ui.gameObject.activeSelf)
            {
                return ui as T;
            }

            if (ui.UIType == UIType.Popup)
            {
                return instance.ShowPopup<T>(ui as UI_Popup);
            }
            else
            {
                ui.gameObject.SetActive(true);
                return ui as T;
            }
        }
        else
        {
            Debug.LogWarning($"[UIManager.Show] {typeof(T)} is not registered.");
            return null;
        }
    }

    public static void Hide<T>() where T : UI_View
    {
        var instance = Instance;

        if (instance._objects.TryGetValue(typeof(T), out var ui))
        {
            if (!ui.gameObject.activeSelf)
            {
                return;
            }

            if (ui.UIType == UIType.Popup)
            {
                instance.HidePopup(ui as UI_Popup);
            }
            else
            {
                ui.gameObject.SetActive(false);
            }
        }
        else
        {
            Debug.LogWarning($"[UIManager.Hide] {typeof(T)} is not registered.");
        }
    }

    public static void HideTopPopup()
    {
        var instance = Instance;

        if (instance._activePopups.Count > 0)
        {
            instance.HidePopup(instance._activePopups.First.Value);
        }
    }

    public static void HideAll(UIType uiType)
    {
        var instance = Instance;

        if (uiType == UIType.Popup)
        {
            foreach (var popup in instance._activePopups)
            {
                popup.gameObject.SetActive(false);
            }

            instance._activePopups.Clear();
            instance._helperPopup = null;
            instance._selfishPopup = null;
        }
        else
        {
            foreach (Transform child in instance._roots[uiType])
            {
                child.gameObject.SetActive(false);
            }
        }
    }

    public static void ShowOrHide<T>() where T : UI_View
    {
        if (IsActive<T>())
        {
            Hide<T>();
        }
        else
        {
            Show<T>();
        }
    }

    public static T Get<T>() where T : UI_View
    {
        if (Instance._objects.TryGetValue(typeof(T), out var ui))
        {
            return ui as T;
        }

        return null;
    }

    public static bool Contains<T>() where T : UI_View
    {
        return Instance._objects.ContainsKey(typeof(T));
    }

    public static bool IsActive<T>() where T : UI_View
    {
        return Instance._objects.TryGetValue(typeof(T), out var ui) && ui.gameObject.activeSelf;
    }

    public static void Clear()
    {
        var instance = Instance;

        foreach (var ui in instance._objects.Values)
        {
            Destroy(ui.gameObject);
        }

        instance._objects.Clear();
        instance._activePopups.Clear();
        instance._helperPopup = null;
        instance._selfishPopup = null;
    }

    private void RegisterPopup(UI_Popup popup)
    {
        popup.Focused += () =>
        {
            if (_activePopups.First.Value == popup)
            {
                return;
            }

            _activePopups.Remove(popup);
            _activePopups.AddFirst(popup);
            RefreshAllPopupDepth();
        };

        popup.Showed += () =>
        {
            InputManager.CursorLocked = false;
        };

        popup.Hided += () =>
        {
            if (_activePopups.Count == 0)
            {
                InputManager.CursorLocked = true;
            }
        };
    }

    private void UnregisterPopup(UI_Popup popup)
    {
        if (popup.IsHelper && _helperPopup == popup)
        {
            _helperPopup = null;
        }
        else if (popup.IsSelfish && _selfishPopup == popup)
        {
            _selfishPopup = null;
        }

        _activePopups.Remove(popup);
    }

    private T ShowPopup<T>(UI_Popup popup) where T : UI_View
    {
        if (IsActiveSelfishPopup && !popup.IgnoreSelfish)
        {
            return null;
        }

        if (popup.IsHelper)
        {
            if (IsActiveHelperPopup)
            {
                _activePopups.Remove(_helperPopup);
                _helperPopup.gameObject.SetActive(false);
            }

            _helperPopup = popup;
        }
        else if (popup.IsSelfish)
        {
            HideAll(UIType.Popup);
            _selfishPopup = popup;
        }

        _activePopups.AddFirst(popup);
        RefreshAllPopupDepth();
        popup.gameObject.SetActive(true);

        return popup as T;
    }

    private void HidePopup(UI_Popup popup)
    {
        if (popup.IsHelper)
        {
            _helperPopup = null;
        }
        else if (popup.IsSelfish)
        {
            _selfishPopup = null;
        }

        _activePopups.Remove(popup);
        popup.gameObject.SetActive(false);
    }

    private void RefreshAllPopupDepth()
    {
        int count = 1;
        foreach (var popup in _activePopups)
        {
            popup.Canvas.sortingOrder = (int)UIType.Top - count++;
        }
    }
}
