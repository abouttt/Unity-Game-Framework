using UnityEngine;

public static class Managers
{
    public static PoolManager Pool => PoolManager.Instance;

    private static bool _initialized;

    public static void Init()
    {
        if (_initialized)
        {
            return;
        }

        var root = new GameObject("Managers");
        Object.DontDestroyOnLoad(root);

        Pool.transform.SetParent(root.transform);

        _initialized = true;
    }

    public static void Clear()
    {
        Pool.Clear();
    }
}
