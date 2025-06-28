using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "GameSettings", menuName = "Settings/Game Settings")]
public class GameSettings : ScriptableObject
{
    [field: Header("Input Settings")]
    [field: SerializeField]
    public InputActionAsset InputActions { get; private set; }

    [field: Header("Sound Settings")]
    [field: SerializeField]
    public AudioMixer AudioMixer { get; private set; }

    [field: SerializeField]
    public GameObject DDDAudioPrefab { get; private set; }
}
