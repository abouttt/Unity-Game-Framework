using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameFramework.Input
{
    public sealed class InputManager
    {
        public InputActionAsset ActionAsset { get; private set; }

        private const string SaveKey = "InputBindings";

        public InputManager()
        {
            ActionAsset = InputSystem.actions;
            ActionAsset.Disable();
        }

        public InputActionMap FindActionMap(string nameOrId)
        {
            return ActionAsset.FindActionMap(nameOrId, true);
        }

        public InputAction FindAction(string actionMapNameOrId, string actionNameOrId)
        {
            return FindActionMap(actionMapNameOrId)?.FindAction(actionNameOrId, true);
        }

        public void EnableActionMap(string nameOrId)
        {
            FindActionMap(nameOrId)?.Enable();
        }

        public void DisableActionMap(string nameOrId)
        {
            FindActionMap(nameOrId)?.Disable();
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

        public async Awaitable RebindingAsync(string actionMapNameOrId, string actionNameOrId, int bindingIndex,
            string cancelBinding, Action onComplete = null)
        {
            var action = FindAction(actionMapNameOrId, actionNameOrId);
            if (!IsValidBindingIndex(action, bindingIndex))
            {
                return;
            }

            var tcs = new AwaitableCompletionSource();

            action.PerformInteractiveRebinding(bindingIndex)
                  .WithCancelingThrough(cancelBinding)
                  .OnComplete(op =>
                  {
                      op.Dispose();
                      onComplete?.Invoke();
                      tcs.SetResult();
                  })
                  .OnCancel(op =>
                  {
                      op.Dispose();
                      tcs.SetCanceled();
                  })
                  .Start();

            await tcs.Awaitable;
        }

        public string GetBindingDisplayName(string actionMapNameOrId, string actionNameOrId, int bindingIndex)
        {
            var action = FindAction(actionMapNameOrId, actionNameOrId);
            if (!IsValidBindingIndex(action, bindingIndex))
            {
                return "N/A";
            }

            return action.GetBindingDisplayString(bindingIndex);
        }

        public void SaveBindings()
        {
            var json = ActionAsset.SaveBindingOverridesAsJson();
            PlayerPrefs.SetString(SaveKey, json);
            PlayerPrefs.Save();
        }

        public void LoadBindings()
        {
            if (!PlayerPrefs.HasKey(SaveKey))
            {
                return;
            }

            var json = PlayerPrefs.GetString(SaveKey);
            ActionAsset.LoadBindingOverridesFromJson(json);
        }

        public void ResetAllBindings()
        {
            ActionAsset.RemoveAllBindingOverrides();
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

        private bool IsValidBindingIndex(InputAction action, int bindingIndex)
        {
            return action != null && bindingIndex >= 0 && bindingIndex < action.bindings.Count;
        }
    }
}
