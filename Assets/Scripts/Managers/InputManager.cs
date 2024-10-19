using UnityEngine;
using UnityEngine.InputSystem;

public sealed class InputManager : MonoBehaviourSingleton<InputManager>
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
        get => _cursorLocked;
        set
        {
            _cursorLocked = value;
            Cursor.visible = !value;
            Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
        }
    }

    private InputActions _inputActions;
    private bool _cursorLocked;

    protected override void Init()
    {
        base.Init();
        _inputActions = new();
        Enabled = false;
        CursorLocked = false;
    }

    protected override void Dispose()
    {
        base.Dispose();
        _inputActions.Dispose();
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
        string bindingPath = GetAction(actionNameOrId).bindings[bindingIndex].path;
        int lastSlashIndex = bindingPath.LastIndexOf('/');
        string path = lastSlashIndex >= 0 ? bindingPath[(lastSlashIndex + 1)..] : bindingPath;
        return path.ToUpper();
    }

    public void Clear()
    {
        Enabled = false;
    }
}
