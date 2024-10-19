using System;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper.SerializedCollections;

public sealed class PoolManager : MonoBehaviourSingleton<PoolManager>
{
    #region Pool
    [Serializable]
    private class Pool
    {
        public Transform Root => _root;
        public int Count => _count;

        [SerializeField, ReadOnly]
        private GameObject _prefab;

        [SerializeField, ReadOnly]
        private int _count;

        private readonly Transform _root;
        private readonly HashSet<GameObject> _activeObjects = new();
        private readonly Stack<GameObject> _inactiveObjects = new();

        public Pool(GameObject prefab, int count, string tag)
        {
            _prefab = prefab;
            _root = new GameObject($"{tag}_Root").transform;

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
            _count = 0;
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
            PushToInactiveContainer(go);
            _count++;
        }

        private void PushToInactiveContainer(GameObject go)
        {
            go.SetActive(false);
            go.transform.SetParent(_root);
            _inactiveObjects.Push(go);
        }
    }
    #endregion

    [SerializeField, ReadOnly, SerializedDictionary("Tag", "Pool")]
    private SerializedDictionary<string, Pool> _pools = new();

    protected override void Dispose()
    {
        base.Dispose();
        Clear();
    }

    public void CreatePool(GameObject prefab, int count = 3, string tag = null)
    {
        if (string.IsNullOrEmpty(tag))
        {
            tag = prefab.name;
        }

        if (_pools.ContainsKey(tag))
        {
            Debug.LogWarning($"[PoolManager.CreatePool] {tag} pool already exists.");
            return;
        }

        var pool = new Pool(prefab, count, tag);
        pool.Root.SetParent(transform);
        _pools.Add(tag, pool);
    }

    public GameObject Get(string tag, Transform parent = null)
    {
        if (_pools.TryGetValue(tag, out var pool))
        {
            return pool.Get(parent);
        }
        else
        {
            Debug.LogWarning($"[PoolManager.Get] {tag} pool does not exist.");
            return null;
        }
    }

    public void Release(GameObject go, string tag = null)
    {
        if (string.IsNullOrEmpty(tag))
        {
            tag = go.name;
        }

        if (_pools.TryGetValue(tag, out var pool))
        {
            if (!pool.Release(go))
            {
                Debug.LogWarning($"[PoolManager.Release] {go.name} object is not included in {tag} pool.");
                Destroy(go);
            }
        }
        else
        {
            Debug.LogWarning($"[PoolManager.Release] {tag} pool does not exist.");
            Destroy(go);
        }
    }

    public void ReleaseAll(string tag)
    {
        if (_pools.TryGetValue(tag, out var pool))
        {
            pool.ReleaseAll();
        }
        else
        {
            Debug.LogWarning($"[PoolManager.ReleaseAll] {tag} pool does not exist.");
        }
    }

    public void ClearPool(string tag)
    {
        if (_pools.TryGetValue(tag, out var pool))
        {
            pool.Clear();
        }
        else
        {
            Debug.LogWarning($"[PoolManager.ClearPool] {tag} pool does not exist.");
        }
    }

    public void RemovePool(string tag)
    {
        if (_pools.TryGetValue(tag, out var pool))
        {
            pool.Dispose();
            _pools.Remove(tag);
        }
        else
        {
            Debug.LogWarning($"[PoolManager/RemovePool] {tag} pool does not exist.");
        }
    }

    public void Clear()
    {
        foreach (var kvp in _pools)
        {
            kvp.Value.Dispose();
        }

        _pools.Clear();
    }
}
