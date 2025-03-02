using System.Collections.Generic;
using UnityEngine;
using AYellowpaper.SerializedCollections;

public sealed class PoolManager : IManager
{
    #region Pool
    private class Pool
    {
        public Transform Root => _root;

        private readonly GameObject _prefab;
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

            var gameObject = _inactiveObjects.Pop();
            if (parent != null)
            {
                gameObject.transform.SetParent(parent);
            }
            gameObject.SetActive(true);
            _activeObjects.Add(gameObject);

            return gameObject;
        }

        public bool Release(GameObject gameObject)
        {
            if (!_activeObjects.Remove(gameObject))
            {
                return false;
            }

            ChangeToInactive(gameObject);

            return true;
        }

        public void ReleaseAll()
        {
            foreach (var activeGameObject in _activeObjects)
            {
                ChangeToInactive(activeGameObject);
            }

            _activeObjects.Clear();
        }

        public void Clear()
        {
            foreach (var activeGameObject in _activeObjects)
            {
                Object.Destroy(activeGameObject);
            }

            foreach (var inactiveGameObject in _inactiveObjects)
            {
                Object.Destroy(inactiveGameObject);
            }

            _activeObjects.Clear();
            _inactiveObjects.Clear();
        }

        public void Dispose()
        {
            Clear();
            Object.Destroy(_root.gameObject);
        }

        private void Create()
        {
            var newGameObject = Object.Instantiate(_prefab);
            newGameObject.name = _prefab.name;
            ChangeToInactive(newGameObject);
        }

        private void ChangeToInactive(GameObject gameObject)
        {
            gameObject.SetActive(false);
            gameObject.transform.SetParent(_root);
            _inactiveObjects.Push(gameObject);
        }
    }
    #endregion

    private Transform _root;
    private readonly SerializedDictionary<string, Pool> _pools = new();

    public void Initialize()
    {
        _root = IManager.CreateRoot("Pool_Root");
    }

    public void CreatePool(GameObject prefab, int count = 5)
    {
        var name = prefab.name;

        if (_pools.ContainsKey(name))
        {
            Debug.LogWarning($"[PoolManager] {name} Pool already exist");
            return;
        }

        var newPool = new Pool(prefab, count);
        newPool.Root.SetParent(_root);
        _pools.Add(name, newPool);
    }

    public GameObject Get(GameObject gameObject, Transform parent = null)
    {
        if (!_pools.TryGetValue(gameObject.name, out var pool))
        {
            CreatePool(gameObject);
            pool = _pools[gameObject.name];
        }

        return pool.Get(parent);
    }

    public GameObject Get(string name, Transform parent = null)
    {
        if (_pools.TryGetValue(name, out var pool))
        {
            return pool.Get(parent);
        }
        else
        {
            return null;
        }
    }

    public bool Release(GameObject gameObject)
    {
        if (_pools.TryGetValue(gameObject.name, out var pool))
        {
            if (pool.Release(gameObject))
            {
                return true;
            }
        }

        return false;
    }

    public void ReleaseAll(string name)
    {
        if (_pools.TryGetValue(name, out var pool))
        {
            pool.ReleaseAll();
        }
    }

    public void ClearPool(string name)
    {
        if (_pools.TryGetValue(name, out var pool))
        {
            pool.Clear();
        }
    }

    public void RemovePool(string name)
    {
        if (_pools.TryGetValue(name, out var pool))
        {
            pool.Dispose();
            _pools.Remove(name);
        }
    }

    public bool Contains(string name)
    {
        return _pools.ContainsKey(name);
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
