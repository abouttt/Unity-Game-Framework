using UnityEngine;

public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static readonly object _lock = new();
    private static bool _isApplicationQuitting = false;

    public static T Instance
    {
        get
        {
            if (_isApplicationQuitting)
            {
                Debug.LogWarning($"Instance of {typeof(T)} is already destroyed. Returning null.");
                return null;
            }

            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<T>();
                    if (_instance == null)
                    {
                        GameObject singletonObject = new(typeof(T).Name, typeof(T));
                    }
                }

                return _instance;
            }
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Debug.LogWarning($"An instance of {typeof(T)} already exists. Destroying duplicate instance.");
            Destroy(gameObject);
        }
    }

    protected virtual void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    protected virtual void OnApplicationQuit()
    {
        _isApplicationQuitting = true;
    }
}
