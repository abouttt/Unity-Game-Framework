using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public sealed class UIManager : IManager
{
    public int Count => _objects.Count;
    public int ActivePopupCount => _activePopups.Count;
    public bool IsActiveHelperPopup => _helperPopup != null;
    public bool IsActiveSelfishPopup => _selfishPopup != null;

    private Transform _root;
    private readonly Dictionary<UIType, Transform> _roots = new();
    private readonly Dictionary<Type, UI_View> _objects = new();
    private readonly LinkedList<UI_Popup> _activePopups = new();
    private UI_Popup _helperPopup;
    private UI_Popup _selfishPopup;

    public void Initialize()
    {
        _root = IManager.CreateRoot("UI_Root");
        CreateUIRoots();
    }

    public void Add(UI_View view)
    {
        var viewType = view.GetType();

        if (!_objects.TryGetValue(viewType, out _))
        {
            if (view.UIType == UIType.Popup)
            {
                AddPopup(view as UI_Popup);
            }

            view.transform.SetParent(_roots[view.UIType]);
            _objects.Add(viewType, view);
        }
        else
        {
            Object.Destroy(view.gameObject);
            Debug.LogWarning($"[UIManager] {viewType} is destroyed, because it already exist");
        }
    }

    public void Remove<T>() where T : UI_View
    {
        if (_objects.TryGetValue(typeof(T), out var view))
        {
            if (view.UIType == UIType.Popup)
            {
                RemovePopup(view as UI_Popup);
            }

            _objects.Remove(typeof(T));
            Object.Destroy(view.gameObject);
        }
    }

    public T Show<T>() where T : UI_View
    {
        if (_objects.TryGetValue(typeof(T), out var view))
        {
            if (view.gameObject.activeSelf)
            {
                return view as T;
            }

            if (view.UIType == UIType.Popup)
            {
                return ShowPopup<T>(view as UI_Popup);
            }
            else
            {
                view.gameObject.SetActive(true);
                return view as T;
            }
        }
        else
        {
            Debug.LogWarning($"[UIManager] {typeof(T)} does not exist");
            return null;
        }
    }

    public void Hide<T>() where T : UI_View
    {
        if (_objects.TryGetValue(typeof(T), out var ui))
        {
            if (!ui.gameObject.activeSelf)
            {
                return;
            }

            if (ui.UIType == UIType.Popup)
            {
                HidePopup(ui as UI_Popup);
            }
            else
            {
                ui.gameObject.SetActive(false);
            }
        }
        else
        {
            Debug.LogWarning($"[UIManager] {typeof(T)} does not exist");
        }
    }

    public void HideAll(UIType uiType)
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

    public void HideTopPopup()
    {
        if (_activePopups.Count > 0)
        {
            HidePopup(_activePopups.First.Value);
        }
    }

    public void Toggle<T>() where T : UI_View
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

    public T Get<T>() where T : UI_View
    {
        if (_objects.TryGetValue(typeof(T), out var view))
        {
            return view as T;
        }

        return null;
    }

    public bool Contains<T>() where T : UI_View
    {
        return _objects.ContainsKey(typeof(T));
    }

    public bool IsActive<T>() where T : UI_View
    {
        return _objects.TryGetValue(typeof(T), out var view) && view.gameObject.activeSelf;
    }

    public void Clear()
    {
        foreach (var ui in _objects.Values)
        {
            Object.Destroy(ui.gameObject);
        }

        _objects.Clear();
        _activePopups.Clear();
        _helperPopup = null;
        _selfishPopup = null;
    }

    private void AddPopup(UI_Popup popup)
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
            Managers.Input.CursorLocked = false;
        };

        popup.Hided += () =>
        {
            if (_activePopups.Count == 0)
            {
                Managers.Input.CursorLocked = true;
            }
        };
    }

    private void RemovePopup(UI_Popup popup)
    {
        if (popup.gameObject.activeSelf)
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
        }
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

    private void CreateUIRoots()
    {
        var types = Enum.GetValues(typeof(UIType));
        for (int i = types.Length - 1; i >= 0; i--)
        {
            var type = (UIType)types.GetValue(i);
            var newGameObject = new GameObject($"{type}_Root");
            var root = newGameObject.transform;
            root.SetParent(_root);
            _roots.Add(type, root);
        }
    }
}
