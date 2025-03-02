using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class DataBinder
{
    private readonly Dictionary<Type, Dictionary<string, Object>> _bindings = new();

    public DataBinder(GameObject gameObject)
    {
        FindDataBindings(gameObject);
    }

    public T Get<T>(string id) where T : Object
    {
        if (_bindings.TryGetValue(typeof(T), out var type))
        {
            if (type.TryGetValue(id, out var component))
            {
                return component as T;
            }
        }

        return null;
    }

    private void FindDataBindings(GameObject gameObject)
    {
        foreach (var binding in gameObject.GetComponentsInChildren<DataBinding>(true))
        {
            if (IsNullBinding(binding))
            {
                LogBindingFailed(gameObject, $"{binding} ID or Target is null");
                continue;
            }

            AddBinding(gameObject, binding);
        }
    }

    private void AddBinding(GameObject gameObject, DataBinding binding)
    {
        if (!_bindings.TryGetValue(binding.BindingType, out var typeBindings))
        {
            typeBindings = new();
            _bindings.Add(binding.BindingType, typeBindings);
        }

        if (typeBindings.TryGetValue(binding.DataID, out var existing))
        {
            LogBindingFailed(gameObject, $"Duplicate ID ({binding.DataID}) already bound to {existing.name}");
            return;
        }

        typeBindings.Add(binding.DataID, binding.Target);
    }

    private bool IsNullBinding(DataBinding binding)
    {
        return string.IsNullOrEmpty(binding.DataID) || binding.Target == null;
    }

    private void LogBindingFailed(GameObject gameObject, string message)
    {
        Debug.LogWarning($"[DataBinder] {gameObject.name} binding failed: {message}");
    }
}
