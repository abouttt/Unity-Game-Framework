using System;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper.SerializedCollections;

public sealed class PoolManager : MonoBehaviourSingleton<PoolManager>
{
    #region
    [Serializable]
    private class Pool
    {
        public Transform Root => _root;

        [SerializeField, ReadOnly]
        private GameObject _prefab;

        private readonly Transform _root;
        private readonly HashSet<GameObject> _activeObjects = new();
        private readonly Stack<GameObject> _inactiveObjects = new();

        public Pool(GameObject prefab, int count)
        {
            _prefab = prefab;
            _root = new GameObject($"{prefab}_Root").transform;

            for (int i = 0; i < count; i++)
            {
                Create();
            }
        }

        public GameObject Get(Transform parent)
        {
            if (_inactiveObjects.Count == 0)
            {
                Create();
            }

            var go = _inactiveObjects.Pop();
            go.transform.SetParent(parent != null ? parent : _root);
            go.SetActive(true);
            _activeObjects.Add(go);

            return go;
        }

        public bool Release(GameObject go)
        {
            if (!_activeObjects.Remove(go))
            {
                return false;
            }

            PushToInactiveContainer(go);

            return true;
        }

        public void ReleaseAll()
        {
            foreach (var go in _activeObjects)
            {
                PushToInactiveContainer(go);
            }

            _activeObjects.Clear();
        }

        public void Clear()
        {
            foreach (var go in _activeObjects)
            {
                Destroy(go);
            }

            foreach (var go in _inactiveObjects)
            {
                Destroy(go);
            }

            _activeObjects.Clear();
            _inactiveObjects.Clear();
        }

        public void Dispose()
        {
            Clear();
            _prefab = null;
            Destroy(_root.gameObject);
        }

        private void Create()
        {
            var go = Instantiate(_prefab);
            go.name = _prefab.name;
            PushToInactiveContainer(go);
        }

        private void PushToInactiveContainer(GameObject go)
        {
            go.SetActive(false);
            go.transform.SetParent(_root);
            _inactiveObjects.Push(go);
        }
    }
    #endregion

    [SerializeField, ReadOnly, SerializedDictionary("Name", "Pool")]
    private SerializedDictionary<string, Pool> _pools = new();

    protected override void Dispose()
    {
        base.Dispose();
        Clear();
    }

    public static void CreatePool(GameObject prefab, int count = 5)
    {
        if (prefab == null)
        {
            Debug.LogWarning($"[PoolManager.CreatePool] Prefab is null.");
            return;
        }

        var instance = Instance;
        var name = prefab.name;

        if (instance._pools.ContainsKey(name))
        {
            Debug.LogWarning($"[PoolManager.CreatePool] {name} pool already exist.");
            return;
        }

        var pool = new Pool(prefab, count);
        pool.Root.SetParent(instance.transform);
        instance._pools.Add(name, pool);
    }

    public static GameObject Get(GameObject go, Transform parent = null)
    {
        if (go == null)
        {
            Debug.LogWarning($"[PoolManager.Get] GameObject is null.");
            return null;
        }

        var instance = Instance;

        if (!instance._pools.TryGetValue(go.name, out var pool))
        {
            CreatePool(go);
            pool = instance._pools[go.name];
        }

        return pool.Get(parent);
    }

    public static GameObject Get(string name, Transform parent = null)
    {
        if (Instance._pools.TryGetValue(name, out var pool))
        {
            return pool.Get(parent);
        }
        else
        {
            return null;
        }
    }

    public static bool Release(GameObject go)
    {
        if (!go.activeSelf)
        {
            return false;
        }

        if (Instance._pools.TryGetValue(go.name, out var pool))
        {
            if (pool.Release(go))
            {
                return true;
            }
        }

        return false;
    }

    public static void ReleaseAll(string name)
    {
        if (Instance._pools.TryGetValue(name, out var pool))
        {
            pool.ReleaseAll();
        }
    }

    public static void ClearPool(string name)
    {
        if (Instance._pools.TryGetValue(name, out var pool))
        {
            pool.Clear();
        }
    }

    public static void RemovePool(string name)
    {
        var instance = Instance;

        if (instance._pools.TryGetValue(name, out var pool))
        {
            pool.Dispose();
            instance._pools.Remove(name);
        }
    }

    public static bool Contains(string name)
    {
        return Instance._pools.ContainsKey(name);
    }

    public static void Clear()
    {
        var instance = Instance;

        foreach (var kvp in instance._pools)
        {
            kvp.Value.Dispose();
        }

        instance._pools.Clear();
    }
}
