using UnityEngine;

public class Managers : MonoSingleton<Managers>
{
    public static DataManager Data => Instance._data;
    public static InputManager Input => Instance._input;
    public static PoolManager Pool => Instance._pool;
    public static ResourceManager Resource => Instance._resource;
    public static SceneLoadManager Scene => Instance._scene;
    public static SoundManager Sound => Instance._sound;
    public static UIManager UI => Instance._ui;

    private readonly DataManager _data = new();
    private readonly InputManager _input = new();
    private readonly PoolManager _pool = new();
    private readonly ResourceManager _resource = new();
    private readonly SceneLoadManager _scene = new();
    private readonly SoundManager _sound = new();
    private readonly UIManager _ui = new();

    private static bool _isInitialized;

    public static void Initialize()
    {
        if (_isInitialized)
        {
            return;
        }

        Data.Initialize();
        Input.Initialize();
        Pool.Initialize();
        Resource.Initialize();
        Scene.Initialize();
        Sound.Initialize();
        UI.Initialize();

        _isInitialized = true;
    }

    public static void Clear()
    {
        Data.Clear();
        Input.Clear();
        Pool.Clear();
        Resource.Clear();
        Scene.Clear();
        Sound.Clear();
        UI.Clear();
    }
}
