using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using AYellowpaper.SerializedCollections;
using Object = UnityEngine.Object;

public sealed class ResourceManager : MonoBehaviourSingleton<ResourceManager>
{
    public static int Count => Instance._resources.Count;

    [SerializeField, ReadOnly, SerializedDictionary("Key", "Resource")]
    private SerializedDictionary<string, Object> _resources = new();

    protected override void Init()
    {
        base.Init();
        Addressables.InitializeAsync();
    }

    protected override void Dispose()
    {
        base.Dispose();
        Clear();
    }

    public static void LoadAsync<T>(string key, Action<T> callback) where T : Object
    {
        var instance = Instance;

        if (instance._resources.TryGetValue(key, out var resource))
        {
            callback?.Invoke(resource as T);
        }
        else
        {
            Addressables.LoadAssetAsync<T>(key).Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    if (instance._resources.ContainsKey(key))
                    {
                        Addressables.Release(handle);
                    }
                    else
                    {
                        instance._resources.Add(key, handle.Result);
                    }

                    callback?.Invoke(instance._resources[key] as T);
                }
                else
                {
                    Debug.LogWarning($"[ResourceManager.LoadAsync] Failed to load asset with key : {key}");
                    callback?.Invoke(null);
                }
            };
        }
    }

    public static void LoadAllAsync(string label, Action<Object[]> callback = null)
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
                Debug.LogWarning($"[ResourceManager.LoadAllAsync] Failed to load asset with label : {label}");
                callback?.Invoke(Array.Empty<Object>());
            }
        };
    }

    public static void InstantiateAsync(string key, Action<GameObject> callback = null, Transform parent = null, bool pooling = false)
    {
        LoadAsync<GameObject>(key, prefab =>
        {
            var go = pooling ? PoolManager.Get(prefab, parent) : Instantiate(prefab, parent);
            callback?.Invoke(go);
        });
    }

    public static void InstantiateAsync<T>(string key, Action<T> callback = null, Transform parent = null, bool pooling = false)
        where T : Component
    {
        InstantiateAsync(key, go =>
        {
            var component = go.GetComponent<T>();
            callback?.Invoke(component);
        },
        parent, pooling);
    }

    public static void Release(string key)
    {
        var instance = Instance;

        if (instance._resources.TryGetValue(key, out var resource))
        {
            Addressables.Release(resource);
            instance._resources.Remove(key);
        }
        else
        {
            Debug.LogWarning($"[ResourceManager.Release] Failed to release asset with key : {key}");
        }
    }

    public static void Destroy(GameObject go)
    {
        if (go == null)
        {
            return;
        }

        if (PoolManager.Release(go))
        {
            return;
        }

        Object.Destroy(go);
    }

    public static void Clear()
    {
        var instance = Instance;

        foreach (var kvp in instance._resources)
        {
            Addressables.Release(kvp.Value);
        }

        instance._resources.Clear();
    }
}
