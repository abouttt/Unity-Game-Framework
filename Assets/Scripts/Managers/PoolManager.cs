using AYellowpaper.SerializedCollections;
using System;
using UnityEditor;
using UnityEngine;

public partial class PoolManager : MonoSingleton<PoolManager>
{
    private readonly SerializedDictionary<Type, object> _genericPools = new();

    public void CreatePool<T>(T prefab, int count = 5) where T : Component
    {
        if (prefab == null)
        {
            Debug.LogWarning("[PoolManager] Prefab cannot be null.");
            return;
        }

        var type = typeof(T);

        if (_genericPools.ContainsKey(type))
        {
            Debug.LogWarning($"[PoolManager] Generic pool for {type.Name} already exists.");
            return;
        }

        var pool = new ObjectPool<T>(prefab, count, transform);
        _genericPools[type] = pool;
    }

    public T Get<T>(T prefab, Transform parent = null, bool autoCreate = true) where T : Component
    {
        if (prefab == null)
        {
            Debug.LogWarning("[PoolManager] Prefab cannot be null.");
            return null;
        }

        var type = typeof(T);

        if (!_genericPools.TryGetValue(type, out var poolObj))
        {
            if (!autoCreate)
            {
                Debug.LogWarning($"[PoolManager] Generic pool for {type.Name} not found.");
                return null;
            }

            CreatePool(prefab);
            poolObj = _genericPools[type];
        }

        return ((ObjectPool<T>)poolObj).Get(parent);
    }

    public bool Return<T>(T obj) where T : Component
    {
        if (obj == null)
        {
            return false;
        }

        var type = typeof(T);

        if (_genericPools.TryGetValue(type, out var poolObj))
        {
            if (((ObjectPool<T>)poolObj).Return(obj))
            {
                return true;
            }
        }

        Debug.LogWarning($"[PoolManager] Generic pool for {type.Name} not found for return.");
        return false;
    }

    public void ReturnAll<T>() where T : Component
    {
        if (_genericPools.TryGetValue(typeof(T), out var poolObj))
        {
            ((ObjectPool<T>)poolObj).ReturnAll();
        }
    }

    public void ClearGeneric<T>() where T : Component
    {
        if (_genericPools.TryGetValue(typeof(T), out var poolObj))
        {
            ((ObjectPool<T>)poolObj).Clear();
        }
    }

    public void ClearAllGeneric()
    {
        foreach (var poolObj in _genericPools.Values)
        {
            var method = poolObj.GetType().GetMethod("Clear");
            method?.Invoke(poolObj, null);
        }
    }

    public void DisposeGeneric<T>() where T : Component
    {
        var type = typeof(T);
        if (_genericPools.TryGetValue(type, out var poolObj))
        {
            ((ObjectPool<T>)poolObj).Dispose();
            _genericPools.Remove(type);
        }
    }

    public void DisposeAllGeneric()
    {
        foreach (var poolObj in _genericPools.Values)
        {
            var method = poolObj.GetType().GetMethod("Dispose");
            method?.Invoke(poolObj, null);
        }

        _genericPools.Clear();
    }

    public void Clear()
    {
        DisposeAllGeneric();
        _genericPools.Clear();
    }
}
