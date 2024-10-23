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

    public void Save(string saveKey, JToken saveData)
    {
        if (_saveData.ContainsKey(saveKey))
        {
            _saveData[saveKey] = saveData;
        }
        else
        {
            _saveData.Add(saveKey, saveData);
        }

        SaveToFile(SaveFilePath, _saveData.ToString());
    }

    public bool Load<T>(string saveKey, out T saveData) where T : class
    {
        saveData = null;

        if (_saveData != null)
        {
            var token = _saveData.GetValue(saveKey);
            if (token != null)
            {
                saveData = token.ToObject<T>();
            }
        }

        return saveData != null;
    }

    public void DeleteSaveData()
    {
        File.Delete(SaveFilePath);
        File.Delete(SaveMetaFilePath);
        _saveData?.RemoveAll();
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
