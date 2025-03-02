using System;
using UnityEngine;

public class LoadingScene : BaseScene
{
    [SerializeField]
    private string _defaultSceneName;

    protected override void Initialize()
    {
        base.Initialize();
        Managers.Clear();
        Resources.UnloadUnusedAssets();
        GC.Collect();

        Managers.Scene.Loaded += OnSceneLoaded;
        if (!Managers.Scene.IsPrepare)
        {
            Managers.Scene.PrepareLoad(_defaultSceneName);
        }
    }

    private void Start()
    {
        LoadResourcesByLabels(Managers.Scene.StartLoad);
    }

    private async void OnSceneLoaded()
    {
        await SwitchLoadedScene();
        Managers.Scene.Loaded -= OnSceneLoaded;
    }

    private void LoadResourcesByLabels(Action callback = null)
    {
        var loadResourceLabels = Settings.Scene[Managers.Scene.CurrentSceneName].AddressableLabels;
        if (loadResourceLabels == null || loadResourceLabels.Length == 0)
        {
            return;
        }

        int totalCount = loadResourceLabels.Length;
        int loadedCount = 0;

        foreach (var label in loadResourceLabels)
        {
            Managers.Resource.LoadAllAsync(label.labelString, _ =>
            {
                if (++loadedCount == totalCount)
                {
                    callback?.Invoke();
                }
            });
        }
    }

    private async Awaitable SwitchLoadedScene()
    {
        await Awaitable.WaitForSecondsAsync(Settings.Scene.FadeOutDuration);
        Managers.Scene.SwitchLoadedScene();
    }
}
