using System.Collections.Generic;
using UnityEngine;

namespace GameFramework
{
    public sealed class PoolManager : IService
    {
        private readonly Dictionary<string, ObjectPool> _pools = new();

        public void OnBind() { }
        public void OnUnbind() { }

        public void CreatePool(string key, GameObject prefab, int preloadCount = 0)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("[PoolManager] Key cannot be null or empty.");
                return;
            }

            if (prefab == null)
            {
                Debug.LogError("[PoolManager] Prefab cannot be null.");
                return;
            }

            if (_pools.ContainsKey(key))
            {
                Debug.LogWarning($"[PoolManager] Pool with key '{key}' already exists.");
                return;
            }

            var pool = new ObjectPool(key, prefab, preloadCount);
            _pools[key] = pool;
        }

        public PoolObject Get(string key, Transform parent = null)
        {
            if (!_pools.TryGetValue(key, out var pool))
            {
                Debug.LogWarning($"[PoolManager] Pool with key '{key}' does not exist.");
                return null;
            }

            return pool.Get(parent);
        }

        public T Get<T>(string key, Transform parent = null) where T : PoolObject
        {
            var poolObj = Get(key, parent);
            if (poolObj != null)
            {
                return poolObj as T;
            }

            return null;
        }

        public bool Return(string key, PoolObject poolObj)
        {
            if (_pools.TryGetValue(key, out var pool))
            {
                if (pool.Return(poolObj))
                {
                    return true;
                }
                else
                {
                    Debug.LogWarning($"[PoolManager] PoolObject '{poolObj.name}' is not part of the pool '{key}'.");
                }
            }
            else
            {
                Debug.LogWarning($"[PoolManager] Pool with key '{key}' does not exist.");
            }

            return false;
        }

        public void ReturnAll(string key)
        {
            if (_pools.TryGetValue(key, out var pool))
            {
                pool.ReturnAll();
            }
            else
            {
                Debug.LogWarning($"[PoolManager] Pool with key '{key}' does not exist.");
            }
        }

        public void RemovePool(string key)
        {
            if (_pools.TryGetValue(key, out var pool))
            {
                pool.Dispose();
                _pools.Remove(key);
            }
            else
            {
                Debug.LogWarning($"[PoolManager] Pool with key '{key}' does not exist.");
            }
        }

        public bool HasPool(string key)
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
}
