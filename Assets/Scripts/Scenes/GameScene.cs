using UnityEngine;

public class GameScene : BaseScene
{
    [field: SerializeField]
    private AudioClip _bgm;

    private void Start()
    {
        InputManager.Enabled = true;
        InputManager.CursorLocked = true;
        SoundManager.Play2D(_bgm, SoundType.BGM);
    }
}
