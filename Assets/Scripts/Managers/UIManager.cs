using System;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviourSingleton<UIManager>
{
    public int Count => _objects.Count;
    public int ActivePopupCount => _activePopups.Count;
    public bool IsActiveHelperPopup => _helperPopup != null;
    public bool IsActiveSelfishPopup => _selfishPopup != null;

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

    public void Register<T>(T ui) where T : UI_View
    {
        if (!_objects.TryGetValue(typeof(T), out _))
        {
            if (ui.UIType == UIType.Popup)
            {
                RegisterPopup(ui as UI_Popup);
            }

            ui.transform.SetParent(_roots[ui.UIType]);
            _objects.Add(typeof(T), ui);
        }
        else
        {
            Debug.LogWarning($"[UIManager.Register] {typeof(T)} is already registered.");
        }
    }

    public void Unregister<T>(bool destory = false) where T : UI_View
    {
        if (_objects.TryGetValue(typeof(T), out var ui))
        {
            if (ui.UIType == UIType.Popup)
            {
                UnregisterPopup(ui as UI_Popup);
            }

            ui.transform.SetParent(null);
            _objects.Remove(typeof(T));

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

    public T Show<T>() where T : UI_View
    {
        if (_objects.TryGetValue(typeof(T), out var ui))
        {
            if (ui.gameObject.activeSelf)
            {
                return ui as T;
            }

            if (ui.UIType == UIType.Popup)
            {
                return ShowPopup<T>(ui as UI_Popup);
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

    public void Close<T>() where T : UI_View
    {
        if (_objects.TryGetValue(typeof(T), out var ui))
        {
            if (!ui.gameObject.activeSelf)
            {
                return;
            }

            if (ui.UIType == UIType.Popup)
            {
                ClosePopup(ui as UI_Popup);
            }
            else
            {
                ui.gameObject.SetActive(false);
            }
        }
        else
        {
            Debug.LogWarning($"[UIManager.Close] {typeof(T)} is not registered.");
        }
    }

    public void CloseTopPopup()
    {
        if (ActivePopupCount > 0)
        {
            ClosePopup(_activePopups.First.Value);
        }
    }

    public void CloseAll(UIType uiType)
    {
        if (uiType == UIType.Popup)
        {
            foreach (var popup in _activePopups)
            {
                popup.gameObject.SetActive(false);
            }

            _activePopups.Clear();
            _helperPopup = null;
            _selfishPopup = null;
        }
        else
        {
            foreach (Transform child in _roots[uiType])
            {
                child.gameObject.SetActive(false);
            }
        }
    }

    public void ShowOrClose<T>() where T : UI_View
    {
        if (IsActive<T>())
        {
            Close<T>();
        }
        else
        {
            Show<T>();
        }
    }

    public T Get<T>() where T : UI_View
    {
        if (_objects.TryGetValue(typeof(T), out var ui))
        {
            return ui as T;
        }

        return null;
    }

    public bool Contains<T>() where T : UI_View
    {
        return _objects.ContainsKey(typeof(T));
    }

    public bool IsActive<T>() where T : UI_View
    {
        return _objects.TryGetValue(typeof(T), out var ui) && ui.gameObject.activeSelf;
    }

    public void Clear()
    {
        foreach (var ui in _objects.Values)
        {
            Destroy(ui.gameObject);
        }

        _objects.Clear();
        _activePopups.Clear();
        _helperPopup = null;
        _selfishPopup = null;
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
            CloseAll(UIType.Popup);
            _selfishPopup = popup;
        }

        _activePopups.AddFirst(popup);
        RefreshAllPopupDepth();
        popup.gameObject.SetActive(true);

        return popup as T;
    }

    private void ClosePopup(UI_Popup popup)
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
