using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using AYellowpaper.SerializedCollections;
using Object = UnityEngine.Object;

public class ResourceManager : MonoSingleton<ResourceManager>
{
    public int ResourceCount => _resources.Count;

    [SerializeField, ReadOnly]
    private SerializedDictionary<string, Object> _resources = new();
    private readonly Dictionary<string, List<Action<Object>>> _pendingLoads = new();

    protected override void Awake()
    {
        base.Awake();
        Addressables.InitializeAsync();
    }

    public void LoadAsync<T>(string key, Action<T> callback = null) where T : Object
    {
        if (_resources.TryGetValue(key, out var resource))
        {
            callback?.Invoke(resource as T);
            return;
        }

        if (_pendingLoads.TryGetValue(key, out var callbacks))
        {
            callbacks.Add(obj => callback?.Invoke(obj as T));
            return;
        }

        _pendingLoads[key] = new() { obj => callback?.Invoke(obj as T) };

        Addressables.LoadAssetAsync<T>(key).Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                if (_resources.ContainsKey(key))
                {
                    Addressables.Release(handle);
                    Debug.LogWarning($"[ResourceManager] Duplicate load: {key}. Releasing redundant.");
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
            if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result.Count == 0)
            {
                Debug.LogWarning($"[ResourceManager] Failed to load resources with label: {label}");
                callback?.Invoke(Array.Empty<Object>());
                return;
            }

            int total = handle.Result.Count;
            int loaded = 0;
            var results = new Object[total];

            for (int i = 0; i < total; i++)
            {
                int index = i;
                var key = handle.Result[index].PrimaryKey;

                LoadAsync<Object>(key, obj =>
                {
                    results[index] = obj;
                    loaded++;

                    if (loaded == total)
                    {
                        callback?.Invoke(results);
                    }
                });
            }
        };
    }

    public void InstantiateAsync(string key, Action<GameObject> callback = null, Transform parent = null)
    {
        LoadAsync<GameObject>(key, prefab =>
        {
            var go = Instantiate(prefab, parent);
            callback?.Invoke(go);
        });
    }

    public void InstantiateAsync<T>(string key, Action<T> callback = null, Transform parent = null)
        where T : Component
    {
        InstantiateAsync(key, gameObject =>
        {
            if (!gameObject.TryGetComponent<T>(out var component))
            {
                Debug.LogWarning($"[ResourceManager] Component {typeof(T).Name} not found in {key}");
            }

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
            Debug.LogWarning($"[ResourceManager] Failed to release asset with key: {key}");
        }
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
