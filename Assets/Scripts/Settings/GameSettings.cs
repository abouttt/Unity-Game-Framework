using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "GameSettings", menuName = "Settings/Game Settings")]
public class GameSettings : ScriptableObject
{
    [field: Header("Input Settings")]
    [field: SerializeField]
    public InputActionAsset InputActions { get; private set; }
}
