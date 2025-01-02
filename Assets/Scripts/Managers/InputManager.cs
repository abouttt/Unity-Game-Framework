using UnityEngine;
using UnityEngine.InputSystem;

public sealed class InputManager : MonoBehaviourSingleton<InputManager>
{
    public static bool Enabled
    {
        get => Instance._inputActions.asset.enabled;
        set
        {
            if (value)
            {
                Instance._inputActions.Enable();
            }
            else
            {
                Instance._inputActions.Disable();
            }
        }
    }

    public static bool CursorLocked
    {
        get => Instance._cursorLocked;
        set
        {
            Instance._cursorLocked = value;
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

    public static InputActionMap GetActionMap(string nameOrId, bool throwIfNotFound = false)
    {
        return Instance._inputActions.asset.FindActionMap(nameOrId, throwIfNotFound);
    }

    public static InputAction GetAction(string actionNameOrId, bool throwIfNotFound = false)
    {
        return Instance._inputActions.FindAction(actionNameOrId, throwIfNotFound);
    }

    public static string GetBindingPath(string actionNameOrId, int bindingIndex = 0)
    {
        string bindingPath = GetAction(actionNameOrId).bindings[bindingIndex].path;
        int lastSlashIndex = bindingPath.LastIndexOf('/');
        string path = lastSlashIndex >= 0 ? bindingPath[(lastSlashIndex + 1)..] : bindingPath;
        return path.ToUpper();
    }

    public static void Clear()
    {
        Enabled = false;
    }
}
