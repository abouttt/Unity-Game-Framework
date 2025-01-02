using UnityEngine;

public class MonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static bool _isApplicationQuitted;

    public static T Instance
    {
        get
        {
            if (_isApplicationQuitted)
            {
                return null;
            }

            if (_instance == null)
            {
                _instance = FindAnyObjectByType<T>();
                if (_instance == null)
                {
                    new GameObject(typeof(T).Name, typeof(T));
                }
            }

            return _instance;
        }
    }

    private void Awake()
    {
        Init();
    }

    private void OnDestroy()
    {
        if (!_isApplicationQuitted)
        {
            Dispose();
        }
    }

    private void OnApplicationQuit()
    {
        _isApplicationQuitted = true;
    }

    protected virtual void Init()
    {
        if (_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    protected virtual void Dispose()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}
