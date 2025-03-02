using UnityEngine;

public static class AppInitializer
{
    private static bool _isInitialized = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (_isInitialized)
        {
            return;
        }

        Settings.Initialize();
        Managers.Initialize();

        _isInitialized = true;
    }
}
