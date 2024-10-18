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

        string filePath = string.IsNullOrEmpty(path) ? typeof(T).Name : $"{path}/{typeof(T).Name}";

        _instance = Resources.Load<T>(filePath);
        if (_instance == null)
        {
            CreateFile(path);
        }

        return _instance;
    }

    private static void CreateFile(string path)
    {
#if UNITY_EDITOR
        string assetName = typeof(T).Name;
        string fileDirectory = string.IsNullOrEmpty(path) ? $"Assets/Resources" : $"Assets/Resources/{path}";
        string filePath = $"{fileDirectory}/{assetName}.asset";

        CreateDirectory(fileDirectory);

        _instance = AssetDatabase.LoadAssetAtPath<T>(filePath);
        if (_instance == null)
        {
            _instance = CreateInstance<T>();
            AssetDatabase.CreateAsset(_instance, filePath);
            Debug.Log($"{assetName} was created because the asset did not exist.");
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
