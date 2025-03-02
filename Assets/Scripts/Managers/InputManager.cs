using UnityEngine;
using UnityEngine.InputSystem;

public sealed class InputManager : IManager
{
    public bool Enabled
    {
        get => _inputActions.asset.enabled;
        set
        {
            if (value)
            {
                _inputActions.Enable();
            }
            else
            {
                _inputActions.Disable();
            }
        }
    }

    public bool CursorLocked
    {
        get => Cursor.lockState == CursorLockMode.Locked;
        set
        {
            Cursor.visible = !value;
            Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
        }
    }

    private InputActions _inputActions;

    public void Initialize()
    {
        _inputActions = new();
        Enabled = false;
        CursorLocked = false;
    }

    public InputActionMap GetActionMap(string nameOrId, bool throwIfNotFound = false)
    {
        return _inputActions.asset.FindActionMap(nameOrId, throwIfNotFound);
    }

    public InputAction GetAction(string actionNameOrId, bool throwIfNotFound = false)
    {
        return _inputActions.FindAction(actionNameOrId, throwIfNotFound);
    }

    public string GetBindingPath(string actionNameOrId, int bindingIndex = 0)
    {
        var bindingPath = GetAction(actionNameOrId).bindings[bindingIndex].path;
        int lastSlashIndex = bindingPath.LastIndexOf('/');
        var path = lastSlashIndex >= 0 ? bindingPath[(lastSlashIndex + 1)..] : bindingPath;
        return path.ToUpper();
    }

    public void Clear()
    {
        Enabled = false;
    }
}
