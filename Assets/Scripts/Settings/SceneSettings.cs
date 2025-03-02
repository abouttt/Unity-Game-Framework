using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using AYellowpaper.SerializedCollections;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "SceneSettings", menuName = "Settings/Scene Settings")]

public class SceneSettings : ScriptableObject
{
    [Serializable]
    public class Settings
    {
        [field: SerializeField]
        public AssetLabelReference[] AddressableLabels { get; private set; }

        [field: SerializeField]
        public bool ReloadSceneWhenNoResources { get; private set; } = true;

        [field: SerializeField]
        public AudioClip BGM { get; private set; }

        [field: SerializeField]
        public Sprite LoadingBackground { get; private set; }

        public Settings()
        {
            var defaultLabel = new AssetLabelReference
            {
                labelString = "Default",
            };

            AddressableLabels = new[] { defaultLabel };
        }
    }

    public Settings this[string sceneAddress] => _settings[sceneAddress];

    [field: SerializeField]
    public string LoadingSceneName { get; private set; }

    [field: SerializeField]
    public float LoadingDelay { get; private set; }

    [field: SerializeField]
    public float FadeInDuration { get; private set; }

    [field: SerializeField]
    public float FadeOutDuration { get; private set; }

    [SerializeField, SerializedDictionary("Scene Name", "Settings")]
    private SerializedDictionary<string, Settings> _settings;

#if UNITY_EDITOR
    [ContextMenu("Refresh")]
    private void Refresh()
    {
        foreach (var scene in EditorBuildSettings.scenes)
        {
            var sceneName = System.IO.Path.GetFileNameWithoutExtension(scene.path);
            if (!_settings.ContainsKey(sceneName))
            {
                _settings.Add(sceneName, new());
            }
        }

        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
    }
#endif
}
