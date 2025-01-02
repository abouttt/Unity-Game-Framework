using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ScriptableSingleton<T> : ScriptableObject where T : ScriptableObject
{
    private static T _instance;

    public static T Instance => GetInstance(string.Empty);

    public static T GetInstance(string path)
    {
        if (_instance != null)
        {
            return _instance;
        }

        if (string.IsNullOrEmpty(path))
        {
            T[] assets = Resources.LoadAll<T>("");
            if (assets == null || assets.Length == 0)
            {
                CreateFile(string.Empty);
            }
            else if (assets.Length > 1)
            {
                Debug.LogWarning($"[ScriptableSingleton] Multiple instances of {typeof(T).Name} found in Resources.");
            }

            if (_instance == null || assets.Length > 0)
            {
                _instance = assets[0];
            }
        }
        else
        {
            _instance = Resources.Load<T>($"{path}/{typeof(T).Name}");
            if (_instance == null)
            {
                CreateFile(path);
            }
        }

        return _instance;
    }

    private static void CreateFile(string path)
    {
#if UNITY_EDITOR
        string assetName = typeof(T).Name;
        string directory = string.IsNullOrEmpty(path) ? $"Assets/Resources" : $"Assets/Resources/{path}";
        string filePath = $"{directory}/{assetName}.asset";

        CreateDirectory(directory);

        _instance = AssetDatabase.LoadAssetAtPath<T>(filePath);
        if (_instance == null)
        {
            _instance = CreateInstance<T>();
            AssetDatabase.CreateAsset(_instance, filePath);
            AssetDatabase.SaveAssets();
            Debug.Log($"{typeof(T).Name} asset created at: {filePath}");
        }
#endif
    }

    private static void CreateDirectory(string directory)
    {
#if UNITY_EDITOR
        if (!AssetDatabase.IsValidFolder(directory))
        {
            string[] folders = directory.Split('/');
            string currentPath = folders[0];
            for (int i = 1; i < folders.Length; i++)
            {
                if (!AssetDatabase.IsValidFolder($"{currentPath}/{folders[i]}"))
                {
                    AssetDatabase.CreateFolder(currentPath, folders[i]);
                }

                currentPath = $"{currentPath}/{folders[i]}";
            }
        }
#endif
    }
}
