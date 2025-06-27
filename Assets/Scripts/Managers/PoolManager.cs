using UnityEngine;
using AYellowpaper.SerializedCollections;

public partial class PoolManager : MonoSingleton<PoolManager>
{
    [SerializeField, ReadOnly]
    private SerializedDictionary<string, ObjectPool> _pools = new();

    public void CreatePool(GameObject prefab, int count = 5)
    {
        if (prefab == null)
        {
            return;
        }

        var key = prefab.name;

        if (_pools.ContainsKey(key))
        {
            Debug.LogWarning($"[PoolManager] Pool for {key} already exists.");
            return;
        }

        var pool = new ObjectPool(prefab, count, transform);
        _pools[key] = pool;
    }

    public GameObject Get(GameObject prefab, Transform parent = null, bool autoCreate = true)
    {
        if (prefab == null)
        {
            return null;
        }

        var key = prefab.name;

        if (!_pools.TryGetValue(key, out var pool))
        {
            if (!autoCreate)
            {
                Debug.LogWarning($"[PoolManager] Pool for {key} does not exist and autoCreate is false.");
                return null;
            }

            CreatePool(prefab);
            pool = _pools[key];
        }

        return pool.Get(parent);
    }

    public bool Return(GameObject go)
    {
        if (go == null)
        {
            return false;
        }

        var key = go.name;

        if (_pools.TryGetValue(key, out var pool))
        {
            if (pool.Return(go))
            {
                return true;
            }
        }

        Debug.LogWarning($"[PoolManager] Pool for {key} does not exist or object cannot be returned.");

        return false;
    }

    public void ReturnAll(string key)
    {
        if (_pools.TryGetValue(key, out var pool))
        {
            pool.ReturnAll();
        }
    }

    public void ClearPool(string key)
    {
        if (_pools.TryGetValue(key, out var pool))
        {
            pool.Clear();
        }
    }

    public void RemovePool(string key)
    {
        if (_pools.TryGetValue(key, out var pool))
        {
            pool.Dispose();
            _pools.Remove(key);
        }
    }

    public bool Contains(string key)
    {
        return _pools.ContainsKey(key);
    }

    public void Clear()
    {
        foreach (var pool in _pools.Values)
        {
            pool.Dispose();
        }

        _pools.Clear();
    }
}
