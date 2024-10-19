using UnityEngine;

public static class Managers
{
    public static PoolManager Pool => GetInstance(PoolManager.Instance);
    public static ResourceManager Resource => GetInstance(ResourceManager.Instance);

    private static bool _initialized;

    public static void Init()
    {
        if (_initialized)
        {
            return;
        }

        var managers = new GameObject("Managers");
        Object.DontDestroyOnLoad(managers);

        PoolManager.Instance.transform.SetParent(managers.transform);
        ResourceManager.Instance.transform.SetParent(managers.transform);

        _initialized = true;
    }

    public static void Clear()
    {
        Pool.Clear();
        Resource.Clear();
    }

    private static T GetInstance<T>(T instance) where T : MonoBehaviourSingleton<T>
    {
        Init();
        return instance;
    }
}
