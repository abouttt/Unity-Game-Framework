using UnityEngine;

public static class Settings
{
    public static SceneSettings Scene { get; private set; }
    public static UISettings UI { get; private set; }

    public static void Initialize()
    {
        Scene = Load<SceneSettings>();
        UI = Load<UISettings>();
    }

    private static T Load<T>() where T : ScriptableObject
    {
        var asset = Resources.Load<T>($"Settings/{typeof(T).Name}");
        if (asset == null)
        {
            Debug.LogWarning($"[Settings] Failed load {typeof(T).Name} asset.");
        }

        return asset;
    }
}
