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
        private readonly Dictionary<string, AsyncOperationHandle> _assetHandles = new();
        private readonly Dictionary<string, List<Action<Object>>> _pendingLoads = new();

        public void LoadAsync<T>(string key, Action<T> callback = null) where T : Object
        {
            if (_assetHandles.TryGetValue(key, out var completedHandle))
            {
                callback?.Invoke(completedHandle.Result as T);
                return;
            }

            if (_pendingLoads.TryGetValue(key, out var pendingList))
            {
                pendingList.Add(obj => callback?.Invoke(obj as T));
                return;
            }

            _pendingLoads[key] = new() { obj => callback?.Invoke(obj as T) };

            Addressables.LoadAssetAsync<T>(key).Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    if (_assetHandles.ContainsKey(key))
                    {
                        Debug.LogWarning($"[ResourceManager] Resource already exists for key: {key}");
                        Addressables.Release(handle);
                    }
                    else
                    {
                        _assetHandles[key] = handle;
                    }
                }
                else
                {
                    Debug.LogWarning($"[ResourceManager] Failed to load resource with key: {key}");
                    Addressables.Release(handle);
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
            if (_assetHandles.TryGetValue(key, out var handle))
            {
                _assetHandles.Remove(key);
                Addressables.Release(handle);
            }
            else
            {
                Debug.LogWarning($"[ResourceManager] Failed to release resource with key: {key}");
            }
        }

        public bool IsLoaded(string key)
        {
            return _assetHandles.ContainsKey(key);
        }

        public void Clear()
        {
            foreach (var handle in _assetHandles.Values)
            {
                Addressables.Release(handle);
            }

            _assetHandles.Clear();
            _pendingLoads.Clear();
        }
    }
}
