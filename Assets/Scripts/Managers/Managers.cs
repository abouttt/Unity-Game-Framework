using UnityEngine;

public static class Managers
{
    public static InputManager Input => GetInstance(InputManager.Instance);
    public static PoolManager Pool => GetInstance(PoolManager.Instance);
    public static ResourceManager Resource => GetInstance(ResourceManager.Instance);

    private static bool _initialized;

    public static void Init()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;

        var root = new GameObject("Managers").transform;
        Object.DontDestroyOnLoad(root);

        Input.transform.SetParent(root);
        Pool.transform.SetParent(root);
        Resource.transform.SetParent(root);
    }

    public static void Clear()
    {
        Input.Clear();
        Pool.Clear();
        Resource.Clear();
    }

    private static T GetInstance<T>(T instance) where T : MonoBehaviourSingleton<T>
    {
        Init();
        return instance;
    }
}
