using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoSingleton<InputManager>
{
    public bool Enabled
    {
        get => _inputActions.enabled;
        set
        {
            if (_inputActions.enabled != value)
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

    private InputActionAsset _inputActions;
    private InputActionRebindingExtensions.RebindingOperation _rebindingOp;
    private string _rebindingTargetPath = null;

    private const string BindingPrefsKey = "InputBindings";

    protected override void Awake()
    {
        base.Awake();
        _inputActions = InputSystem.actions;
        CursorLocked = false;
    }

    public InputActionMap FindActionMap(string nameOrId)
    {
        var actionMap = _inputActions.FindActionMap(nameOrId);
        if (actionMap == null)
        {
            Debug.LogWarning($"[InputManager] InputActionMap '{nameOrId}' not found.");
        }

        return actionMap;
    }

    public InputAction FindAction(string nameOrId)
    {
        var action = _inputActions.FindAction(nameOrId);
        if (action == null)
        {
            Debug.LogWarning($"[InputManager] InputAction '{nameOrId}' not found.");
        }

        return action;
    }

    public void SwitchActionMap(string nameOrId)
    {
        var actionMap = FindActionMap(nameOrId);
        if (actionMap == null)
        {
            return;
        }

        _inputActions.Disable();
        actionMap.Enable();
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
        string json = _inputActions.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString(BindingPrefsKey, json);
        PlayerPrefs.Save();
        Debug.Log("[InputManager] Input bindings saved.");
    }

    public void LoadBindings()
    {
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
