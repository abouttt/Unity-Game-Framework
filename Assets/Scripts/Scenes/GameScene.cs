using System;
using UnityEngine;

public class GameScene : BaseScene
{
    protected override void Initialize()
    {
        base.Initialize();
        CreateUIFromSettings();
    }

    private void Start()
    {
        Managers.Input.Enabled = true;
        Managers.Input.CursorLocked = true;
        var bgm = Settings.Scene[Managers.Scene.CurrentSceneName].BGM;
        Managers.Sound.Play2D(bgm, SoundType.BGM);
    }

    private void CreateUIFromSettings()
    {
        foreach (UIType uiType in Enum.GetValues(typeof(UIType)))
        {
            foreach (var prefab in Settings.UI[uiType].Prefabs)
            {
                var uiGameObject = Instantiate(prefab);
                var view = uiGameObject.GetComponent<UI_View>();
                Managers.UI.Add(view);
            }
        }
    }
}
