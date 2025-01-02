using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using AYellowpaper.SerializedCollections;

[CreateAssetMenu(fileName = "SceneSettings", menuName = "Settings/Scene Settings")]
public class SceneSettings : ScriptableSingleton<SceneSettings>
{
    [Serializable]
    public class Settings
    {
        [field: SerializeField]
        public AssetLabelReference[] AddressableLabels { get; private set; }

        [field: SerializeField]
        public bool ReloadSceneWhenNoResources { get; private set; }

        [field: SerializeField]
        public Sprite LoadingBackground { get; private set; }
    }

    public Settings this[string sceneAddress] => _settings[sceneAddress];

    [field: SerializeField]
    public float FadeInDuration { get; private set; }

    [field: SerializeField]
    public float FadeOutDuration { get; private set; }

    [SerializeField, SerializedDictionary("Scene Name", "Settings")]
    private SerializedDictionary<string, Settings> _settings;
}
