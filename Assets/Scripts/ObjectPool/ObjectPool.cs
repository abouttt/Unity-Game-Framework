using System.Collections.Generic;
using UnityEngine;

namespace GameFramework
{
    public class ObjectPool
    {
        public int ActiveCount => _activeObjects.Count;
        public int InactiveCount => _inactiveObjects.Count;

        private readonly string _key;
        private readonly GameObject _prefab;
        private readonly Transform _root;
        private readonly HashSet<PoolObject> _activeObjects = new();
        private readonly Stack<PoolObject> _inactiveObjects = new();

        public ObjectPool(string key, GameObject prefab, int preloadCount)
        {
            _key = key;
            _prefab = prefab;
            _root = new GameObject($"Pool_{key}").transform;

            for (int i = 0; i < preloadCount; i++)
            {
                var poolObj = Create();
                Deactivate(poolObj);
            }
        }

        public PoolObject Get(Transform parent)
        {
            PoolObject poolObj = null;

            while (_inactiveObjects.Count > 0)
            {
                poolObj = _inactiveObjects.Pop();
                if (poolObj != null)
                {
                    break;
                }
            }

            if (poolObj == null)
            {
                poolObj = Create();
            }

            Activate(poolObj, parent);

            return poolObj;
        }

        public bool Return(PoolObject poolObj)
        {
            if (!_activeObjects.Remove(poolObj))
            {
                return false;
            }

            Deactivate(poolObj);

            return true;
        }

        public void ReturnAll()
        {
            foreach (var poolObj in _activeObjects)
            {
                Deactivate(poolObj);
            }

            _activeObjects.Clear();
        }

        public void Clear()
        {
            foreach (var poolObj in _activeObjects)
            {
                Release(poolObj);
            }

            foreach (var poolObj in _inactiveObjects)
            {
                Release(poolObj);
            }

            _activeObjects.Clear();
            _inactiveObjects.Clear();
        }

        public void Dispose()
        {
            Clear();
            Object.Destroy(_root.gameObject);
        }

        private PoolObject Create()
        {
            var go = Object.Instantiate(_prefab, _root);
            go.name = _prefab.name;

            var poolObj = go.GetComponent<PoolObject>();
            poolObj.PoolKey = _key;
            poolObj.OnCreate();

            return poolObj;
        }

        private void Release(PoolObject poolObj)
        {
            poolObj.OnRelease();
            Object.Destroy(poolObj.gameObject);
        }

        private void Activate(PoolObject poolObj, Transform parent)
        {
            poolObj.gameObject.SetActive(true);
            poolObj.transform.SetParent(parent);
            poolObj.IsUsing = true;
            poolObj.OnGetFromPool();
            _activeObjects.Add(poolObj);
        }

        private void Deactivate(PoolObject poolObj)
        {
            poolObj.OnReturnToPool();
            poolObj.gameObject.SetActive(false);
            poolObj.transform.SetParent(_root);
            poolObj.IsUsing = false;
            _inactiveObjects.Push(poolObj);
        }
    }
}
