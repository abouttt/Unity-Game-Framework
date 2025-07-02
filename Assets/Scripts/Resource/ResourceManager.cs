using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace GameFramework
{
    public sealed class ResourceManager : IService
    {
        private readonly Dictionary<string, Object> _resources = new();
        private readonly Dictionary<string, List<Action<Object>>> _pendingLoads = new();

        public void OnBind()
        {
            Addressables.InitializeAsync();
        }

        public void OnUnbind()
        {
            Clear();
        }

        public void LoadAsync<T>(string key, Action<T> callback = null) where T : Object
        {
            if (_resources.TryGetValue(key, out var resource))
            {
                callback?.Invoke(resource as T);
                return;
            }

            if (callback != null)
            {
                if (_pendingLoads.ContainsKey(key))
                {
                    _pendingLoads[key].Add(obj => callback?.Invoke(obj as T));
                    return;
                }

                _pendingLoads[key] = new() { obj => callback?.Invoke(obj as T) };
            }

            Addressables.LoadAssetAsync<T>(key).Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    if (_resources.ContainsKey(key))
                    {
                        Addressables.Release(handle);
                        Debug.LogWarning($"[ResourceManager] Resource already exists for key: {key}, releasing old resource.");
                    }
                    else
                    {
                        _resources[key] = handle.Result;
                    }
                }
                else
                {
                    Debug.LogWarning($"[ResourceManager] Failed to load resource with key: {key}");
                }

                if (_pendingLoads.TryGetValue(key, out var callbacks))
                {
                    foreach (var cb in callbacks)
                    {
                        cb?.Invoke(handle.Result);
                    }

                    _pendingLoads.Remove(key);
                }
            };
        }

        public void LoadAllAsync(string label, Action<Object[]> callback = null)
        {
            Addressables.LoadResourceLocationsAsync(label, typeof(Object)).Completed += handle =>
            {
                if (handle.Result.Count != 0)
                {
                    int totalCount = handle.Result.Count;
                    int loadedCount = 0;
                    var resources = new Object[totalCount];

                    foreach (var result in handle.Result)
                    {
                        LoadAsync<Object>(result.PrimaryKey, resource =>
                        {
                            resources[loadedCount++] = resource;
                            if (loadedCount == totalCount)
                            {
                                callback?.Invoke(resources);
                            }
                        });
                    }
                }
                else
                {
                    Debug.LogWarning($"[ResourceManager] Failed to load resources with label: {label}");
                    callback?.Invoke(Array.Empty<Object>());
                }
            };
        }

        public void InstantiateAsync(string key, Action<GameObject> callback = null, Transform parent = null)
        {
            LoadAsync<GameObject>(key, prefab =>
            {
                var go = Object.Instantiate(prefab, parent);
                callback?.Invoke(go);
            });
        }

        public void InstantiateAsync<T>(string key, Action<T> callback = null, Transform parent = null) where T : Component
        {
            InstantiateAsync(key, gameObject =>
            {
                var component = gameObject.GetComponent<T>();
                callback?.Invoke(component);
            },
            parent);
        }

        public void Release(string key)
        {
            if (_resources.TryGetValue(key, out var resource))
            {
                Addressables.Release(resource);
                _resources.Remove(key);
            }
            else
            {
                Debug.LogWarning($"[ResourceManager] Failed to release resource with key: {key}");
            }
        }

        public bool IsLoaded(string key)
        {
            return _resources.ContainsKey(key);
        }

        public void Clear()
        {
            foreach (var resource in _resources.Values)
            {
                Addressables.Release(resource);
            }

            _resources.Clear();
            _pendingLoads.Clear();
        }
    }
}
