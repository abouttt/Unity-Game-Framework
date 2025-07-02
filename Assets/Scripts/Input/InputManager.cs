using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameFramework
{
    public sealed class InputManager : IService
    {
        public event Action<BindingChangeContext> OnBindingChanged;

        private readonly InputActionAsset _inputActions;
        private const string SaveKey = "InputBindings";

        public InputManager(InputActionAsset inputActions)
        {
            _inputActions = inputActions != null ? inputActions : InputSystem.actions;
            _inputActions.Disable();
        }

        public void SetCursorMode(InputCursorMode mode)
        {
            Cursor.visible = mode != InputCursorMode.Locked;
            Cursor.lockState = mode switch
            {
                InputCursorMode.Locked => CursorLockMode.Locked,
                InputCursorMode.Confined => CursorLockMode.Confined,
                _ => CursorLockMode.None
            };
        }

        public void EnableActionMap(string nameOrId)
        {
            FindActionMap(nameOrId)?.Enable();
        }

        public void EnableAllActionMaps()
        {
            foreach (var actionMap in _inputActions.actionMaps)
            {
                actionMap.Enable();
            }
        }

        public void DisableActionMap(string nameOrId)
        {
            FindActionMap(nameOrId)?.Disable();
        }

        public void DisableAllActionMaps()
        {
            foreach (var actionMap in _inputActions.actionMaps)
            {
                actionMap.Disable();
            }
        }

        public InputActionMap FindActionMap(string nameOrId)
        {
            return _inputActions.FindActionMap(nameOrId);
        }

        public InputAction FindAction(string actionMapNameOrId, string actionNameOrId)
        {
            var actionMap = FindActionMap(actionMapNameOrId);
            return actionMap?.FindAction(actionNameOrId);
        }

        public void Rebinding(string actionMapNameOrId, string actionNameOrId, int bindingIndex,
            string cancelBinding, Action onComplete = null, Action onCancel = null)
        {
            var action = FindAction(actionMapNameOrId, actionNameOrId);
            if (!IsValidBindingIndex(action, bindingIndex))
            {
                return;
            }

            action.PerformInteractiveRebinding(bindingIndex)
                  .WithCancelingThrough(cancelBinding)
                  .OnComplete(op =>
                  {
                      op.Dispose();
                      onComplete?.Invoke();
                      var context = new BindingChangeContext
                      {
                          ActionMap = action.actionMap,
                          Action = action,
                          BindingIndex = bindingIndex,
                          NewBindingPath = action.bindings[bindingIndex].effectivePath
                      };
                      OnBindingChanged?.Invoke(context);
                  })
                  .OnCancel(op =>
                  {
                      op.Dispose();
                      onCancel?.Invoke();
                  })
                  .Start();
        }

        public string GetBindingDisplayString(string actionMapNameOrId, string actionNameOrId, int bindingIndex = 0)
        {
            var action = FindAction(actionMapNameOrId, actionNameOrId);
            return IsValidBindingIndex(action, bindingIndex)
                ? action.GetBindingDisplayString(bindingIndex)
                : "N/A";
        }

        public bool HasDuplicateBinding(string actionMapNameOrId, string actionNameOrId, int bindingIndex)
        {
            var targetAction = FindAction(actionMapNameOrId, actionNameOrId);
            if (!IsValidBindingIndex(targetAction, bindingIndex))
            {
                return false;
            }

            var targetBinding = targetAction.bindings[bindingIndex];

            foreach (var binding in targetAction.actionMap.bindings)
            {
                if (binding.action == targetBinding.action)
                {
                    continue;
                }

                if (binding.effectivePath == targetBinding.effectivePath)
                {
                    return true;
                }
            }

            return false;
        }

        public void SaveBindings()
        {
            var json = _inputActions.SaveBindingOverridesAsJson();
            PlayerPrefs.SetString(SaveKey, json);
            PlayerPrefs.Save();
        }

        public void LoadBindings()
        {
            if (PlayerPrefs.HasKey(SaveKey))
            {
                var json = PlayerPrefs.GetString(SaveKey);
                _inputActions.LoadBindingOverridesFromJson(json);
            }
        }

        public void ResetBindings()
        {
            foreach (var actionMap in _inputActions.actionMaps)
            {
                foreach (var action in actionMap.actions)
                {
                    for (int i = 0; i < action.bindings.Count; i++)
                    {
                        if (action.bindings[i].hasOverrides)
                        {
                            action.RemoveBindingOverride(i);
                            var context = new BindingChangeContext
                            {
                                ActionMap = actionMap,
                                Action = action,
                                BindingIndex = i,
                                NewBindingPath = action.bindings[i].effectivePath
                            };
                            OnBindingChanged?.Invoke(context);
                        }
                    }
                }
            }
        }

        private bool IsValidBindingIndex(InputAction action, int bindingIndex)
        {
            return action != null && bindingIndex >= 0 && bindingIndex < action.bindings.Count;
        }
    }
}
