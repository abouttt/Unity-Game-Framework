using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class DataBinder
{
    private readonly Dictionary<Type, Dictionary<string, Object>> _bindings = new();
    private readonly GameObject _gameObject;

    public DataBinder(GameObject gameObject)
    {
        _gameObject = gameObject;
        FindDataBindings();
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

    private void FindDataBindings()
    {
        foreach (var binding in _gameObject.GetComponentsInChildren<DataBinding>(true))
        {
            if (IsNullBinding(binding))
            {
                LogWarning($"Binding failed : ID or Target is null", binding);
                continue;
            }

            AddBinding(binding);
        }
    }

    private void AddBinding(DataBinding binding)
    {
        if (!_bindings.TryGetValue(binding.BindingType, out var typeBindings))
        {
            typeBindings = new Dictionary<string, Object>();
            _bindings[binding.BindingType] = typeBindings;
        }

        if (typeBindings.ContainsKey(binding.DataID))
        {
            LogWarning($"Binding failed : Duplicate ID", binding);
            return;
        }

        typeBindings.Add(binding.DataID, binding.Target);
    }

    private bool IsNullBinding(DataBinding binding)
    {
        return string.IsNullOrEmpty(binding.DataID) || binding.Target == null;
    }

    private void LogWarning(string message, DataBinding binding)
    {
        Debug.LogWarning($"[DataBinder] {_gameObject.name} - {message} (ID : {binding.DataID}, Target : {binding.Target})");
    }
}
