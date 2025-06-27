using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoSingleton<InputManager>
{
    public bool IsEnabled => _inputActions != null && _inputActions.enabled;

    public bool CursorLocked
    {
        get => Cursor.lockState == CursorLockMode.Locked;
        set => SetCursorState(!value, value ? CursorLockMode.Locked : CursorLockMode.None);
    }

    private InputActionAsset _inputActions;
    private InputActionRebindingExtensions.RebindingOperation _rebindingOp;
    private string _rebindingTargetPath = null;
    private IInputContext _currentContext;

    private const string BindingPrefsKey = "InputBindings";

    protected override void Awake()
    {
        base.Awake();
        CursorLocked = false;
    }

    public void Initialize(InputActionAsset inputActions)
    {
        if (inputActions == null)
        {
            Debug.LogError("[InputManager] InputActionAsset is null during initialization.");
            return;
        }

        _inputActions = inputActions;
        _inputActions.Disable();
    }

    public void SetCursorState(bool visible, CursorLockMode mode)
    {
        Cursor.visible = visible;
        Cursor.lockState = mode;
    }

    public void EnableInput(bool enabled)
    {
        if (_inputActions == null)
        {
            return;
        }

        if (enabled)
        {
            _inputActions.Enable();
        }
        else
        {
            _inputActions.Disable();
        }
    }

    public void SwitchContext(IInputContext context)
    {
        if (_inputActions == null)
        {
            Debug.LogWarning("[InputManager] Cannot switch context. InputActionAsset is null.");
            return;
        }

        _inputActions.Disable();

        _currentContext?.Unbind();
        _currentContext = context;
        _currentContext?.Bind();

        _inputActions.Enable();
    }

    public InputActionMap FindActionMap(string nameOrId)
    {
        if (_inputActions == null)
        {
            Debug.LogWarning("[InputManager] InputActionAsset is null.");
            return null;
        }

        var actionMap = _inputActions.FindActionMap(nameOrId);
        if (actionMap == null)
        {
            Debug.LogWarning($"[InputManager] InputActionMap '{nameOrId}' not found.");
        }

        return actionMap;
    }

    public InputAction FindAction(string nameOrId)
    {
        if (_inputActions == null)
        {
            Debug.LogWarning("[InputManager] InputActionAsset is null.");
            return null;
        }

        var action = _inputActions.FindAction(nameOrId);
        if (action == null)
        {
            Debug.LogWarning($"[InputManager] InputAction '{nameOrId}' not found.");
        }

        return action;
    }

    public string GetBindingDisplayName(string actionNameOrId, int bindingIndex = 0)
    {
        var action = FindAction(actionNameOrId);
        if (action == null || bindingIndex >= action.bindings.Count)
        {
            return "N/A";
        }

        return InputControlPath.ToHumanReadableString(
            action.bindings[bindingIndex].effectivePath,
            InputControlPath.HumanReadableStringOptions.OmitDevice
        ).ToUpper();
    }

    public void Rebind(InputAction action, int bindingIndex, Action onComplete, Action onCancel)
    {
        if (action == null || bindingIndex >= action.bindings.Count)
        {
            Debug.LogWarning("[InputManager] Invalid action or binding index.");
            return;
        }

        action.Disable();

        _rebindingTargetPath = action.bindings[bindingIndex].hasOverrides ?
                               action.bindings[bindingIndex].overridePath :
                               action.bindings[bindingIndex].path;

        _rebindingOp = action.PerformInteractiveRebinding(bindingIndex)
                             .OnComplete(_ => OnRebindComplete(action, onComplete, onCancel))
                             .OnCancel(_ => OnRebindCancel(action, onCancel))
                             .Start();
    }

    public void SaveBindings()
    {
        if (_inputActions == null)
        {
            return;
        }

        string json = _inputActions.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString(BindingPrefsKey, json);
        PlayerPrefs.Save();
        Debug.Log("[InputManager] Input bindings saved.");
    }

    public void LoadBindings()
    {
        if (_inputActions == null)
        {
            return;
        }

        if (PlayerPrefs.HasKey(BindingPrefsKey))
        {
            string json = PlayerPrefs.GetString(BindingPrefsKey);
            _inputActions.LoadBindingOverridesFromJson(json);
            Debug.Log("[InputManager] Input bindings loaded.");
        }
    }

    private void OnRebindComplete(InputAction action, Action onComplete, Action onCancel)
    {
        if (HasConflict(action))
        {
            if (!string.IsNullOrEmpty(_rebindingTargetPath))
            {
                action.ApplyBindingOverride(_rebindingTargetPath);
            }

            OnRebindCancel(action, onCancel);
        }
        else
        {
            ResetRebindState();
            action.Enable();
            onComplete?.Invoke();
        }
    }

    private void OnRebindCancel(InputAction action, Action callback)
    {
        ResetRebindState();
        action.Enable();
        callback?.Invoke();
    }

    private bool HasConflict(InputAction action)
    {
        var newBinding = action.bindings[0];
        foreach (var binding in action.actionMap.bindings)
        {
            if (binding.action == newBinding.action)
            {
                continue;
            }

            if (binding.effectivePath == newBinding.effectivePath)
            {
                Debug.Log($"[InputManager] Conflict: {newBinding.effectivePath}");
                return true;
            }
        }

        return false;
    }

    private void ResetRebindState()
    {
        _rebindingOp?.Dispose();
        _rebindingOp = null;
        _rebindingTargetPath = null;
    }
}
