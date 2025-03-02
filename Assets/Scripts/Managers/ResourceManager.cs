using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using AYellowpaper.SerializedCollections;
using Object = UnityEngine.Object;

public sealed class ResourceManager : IManager
{
    public int ResourceCount => _resources.Count;

    private readonly SerializedDictionary<string, Object> _resources = new();

    public void Initialize()
    {
        Addressables.InitializeAsync();
    }

    public void LoadAsync<T>(string key, Action<T> callback = null) where T : Object
    {
        if (_resources.TryGetValue(key, out var resource))
        {
            callback?.Invoke(resource as T);
        }
        else
        {
            Addressables.LoadAssetAsync<T>(key).Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    if (_resources.ContainsKey(key))
                    {
                        Addressables.Release(handle);
                    }
                    else
                    {
                        _resources.Add(key, handle.Result);
                    }

                    callback?.Invoke(_resources[key] as T);
                }
                else
                {
                    Debug.LogWarning($"[ResourceManager] Failed to load asset with key: {key}");
                    Addressables.Release(handle);
                    callback?.Invoke(null);
                }
            };
        }
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
                Debug.LogWarning($"[ResourceManager] Failed to load asset with label: {label}");
                callback?.Invoke(Array.Empty<Object>());
            }
        };
    }

    public void InstantiateAsync(string key, Action<GameObject> callback = null, Transform parent = null, bool pooling = false)
    {
        LoadAsync<GameObject>(key, prefab =>
        {
            var gameObject = pooling ? Managers.Pool.Get(prefab, parent) : Object.Instantiate(prefab, parent);
            callback?.Invoke(gameObject);
        });
    }

    public void InstantiateAsync<T>(string key, Action<T> callback = null, Transform parent = null, bool pooling = false)
        where T : Component
    {
        InstantiateAsync(key, gameObject =>
        {
            var component = gameObject.GetComponent<T>();
            callback?.Invoke(component);
        },
        parent, pooling);
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
            Debug.LogWarning($"[ResourceManager] Failed to release asset with key: {key}");
        }
    }

    public void Destroy(GameObject gameObject)
    {
        if (gameObject == null)
        {
            return;
        }

        if (Managers.Pool.Release(gameObject))
        {
            return;
        }

        Object.Destroy(gameObject);
    }

    public void Clear()
    {
        foreach (var kvp in _resources)
        {
            Addressables.Release(kvp.Value);
        }

        _resources.Clear();
    }
}
