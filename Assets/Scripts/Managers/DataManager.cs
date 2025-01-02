using System.IO;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization.Formatters.Binary;

public class DataManager : MonoBehaviourSingleton<DataManager>
{
    public static readonly string SavePath = $"{Application.streamingAssetsPath}/Saved";
    public static readonly string SaveFilePath = $"{SavePath}/Saves.json";
    public static readonly string SaveMetaFilePath = $"{SavePath}/Saves.meta";
    public static readonly string SettingsSavePath = $"{SavePath}/Settings.json";

    public bool HasSaveData => File.Exists(SaveFilePath);

    private JObject _saveData = new();
    private readonly BinaryFormatter _binaryFormatter = new();

    protected override void Init()
    {
        base.Init();

        var directory = new DirectoryInfo(SavePath);
        if (!directory.Exists)
        {
            directory.Create();
        }

        if (LoadFromFile(SaveFilePath, out var json))
        {
            _saveData = JObject.Parse(json);
        }
    }

    public static void Save(string saveKey, JToken saveData)
    {
        var instance = Instance;

        if (instance._saveData.ContainsKey(saveKey))
        {
            instance._saveData[saveKey] = saveData;
        }
        else
        {
            instance._saveData.Add(saveKey, saveData);
        }

        instance.SaveToFile(SaveFilePath, instance._saveData.ToString());
    }

    public static bool Load<T>(string saveKey, out T saveData) where T : class
    {
        var instance = Instance;
        saveData = null;

        if (instance._saveData != null)
        {
            var token = instance._saveData.GetValue(saveKey);
            if (token != null)
            {
                saveData = token.ToObject<T>();
            }
        }

        return saveData != null;
    }

    public static void DeleteSaveData()
    {
        File.Delete(SaveFilePath);
        File.Delete(SaveMetaFilePath);
        Instance._saveData?.RemoveAll();
    }

    private void SaveToFile(string path, string json)
    {
        using var stream = new FileStream(path, FileMode.Create);
        _binaryFormatter.Serialize(stream, json);
    }

    private bool LoadFromFile(string path, out string json)
    {
        json = null;

        if (File.Exists(path))
        {
            using var stream = new FileStream(path, FileMode.Open);
            json = _binaryFormatter.Deserialize(stream) as string;
            return true;
        }
        else
        {
            Debug.Log($"[DataManager] No have save data : {path}");
            return false;
        }
    }
}
