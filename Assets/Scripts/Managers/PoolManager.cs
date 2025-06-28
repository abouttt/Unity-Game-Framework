using System.Collections.Generic;
using UnityEngine;
using AYellowpaper.SerializedCollections;

public partial class PoolManager : MonoSingleton<PoolManager>
{
    #region Pool
    private class Pool
    {
        private readonly GameObject _prefab;
        private readonly Transform _root;
        private readonly HashSet<GameObject> _activeObjects = new();
        private readonly Stack<GameObject> _inactiveObjects = new();

        public Pool(GameObject prefab, int count, Transform parent)
        {
            _prefab = prefab;
            _root = new GameObject($"{prefab.name}_Pool").transform;
            _root.SetParent(parent);

            for (int i = 0; i < count; i++)
            {
                var go = Create();
                Deactivate(go, false);
            }
        }

        public GameObject Get(Transform parent = null)
        {
            GameObject go = null;

            while (_inactiveObjects.Count > 0)
            {
                go = _inactiveObjects.Pop();
                if (go != null)
                {
                    break;
                }
            }

            if (go == null)
            {
                go = Create();
            }

            if (parent != null)
            {
                go.transform.SetParent(parent);
            }

            go.SetActive(true);

            if (go.TryGetComponent<IPoolable>(out var poolable))
            {
                poolable.OnSpawn();
            }

            _activeObjects.Add(go);

            return go;
        }

        public bool Return(GameObject go)
        {
            if (!_activeObjects.Remove(go))
            {
                return false;
            }

            Deactivate(go);

            return true;
        }

        public void ReturnAll()
        {
            foreach (var go in _activeObjects)
            {
                Deactivate(go);
            }

            _activeObjects.Clear();
        }

        public void Clear()
        {
            foreach (var go in _activeObjects)
            {
                if (go != null)
                {
                    Object.Destroy(go);
                }
            }

            foreach (var go in _inactiveObjects)
            {
                if (go != null)
                {
                    Object.Destroy(go);
                }
            }

            _activeObjects.Clear();
            _inactiveObjects.Clear();
        }

        public void Dispose()
        {
            Clear();

            if (_root != null)
            {
                Object.Destroy(_root.gameObject);
            }
        }

        private GameObject Create()
        {
            var go = Object.Instantiate(_prefab, _root);
            go.name = _prefab.name;
            return go;
        }

        private void Deactivate(GameObject go, bool callDespawn = true)
        {
            if (go != null)
            {
                if (callDespawn && go.TryGetComponent<IPoolable>(out var poolable))
                {
                    poolable.OnDespawn();
                }

                go.SetActive(false);
                go.transform.SetParent(_root);
                _inactiveObjects.Push(go);
            }
        }
    }
    #endregion

    [SerializeField, ReadOnly]
    private SerializedDictionary<string, Pool> _pools = new();

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

        var pool = new Pool(prefab, count, transform);
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
