using System;
using System.Collections;
using UnityEngine;

public class LoadingScene : BaseScene
{
    [SerializeField]
    private string _defaultSceneName;

    protected override void Init()
    {
        base.Init();
        ClearManagers();
        Resources.UnloadUnusedAssets();
        GC.Collect();

        if (!SceneLoader.IsReadyToLoad)
        {
            SceneLoader.ReadyToLoad(_defaultSceneName);
        }

        SceneLoader.LoadCompleted += () => StartCoroutine(LoadComplete());
    }

    private void Start()
    {
        LoadResourcesByLabels(() => SceneLoader.StartLoad());
    }

    private IEnumerator LoadComplete()
    {
        yield return YieldCache.WaitForSeconds(SceneSettings.Instance.FadeOutDuration);
        SceneLoader.SwitchLoadedScene();
    }

    private void LoadResourcesByLabels(Action callback = null)
    {
        var loadResourceLabels = SceneSettings.Instance[SceneLoader.NextSceneName].AddressableLabels;
        if (loadResourceLabels == null || loadResourceLabels.Length == 0)
        {
            callback?.Invoke();
            return;
        }

        int totalCount = loadResourceLabels.Length;
        int loadedCount = 0;

        foreach (var label in loadResourceLabels)
        {
            ResourceManager.LoadAllAsync(label.labelString, _ =>
            {
                if (++loadedCount == totalCount)
                {
                    callback?.Invoke();
                }
            });
        }
    }

    private void ClearManagers()
    {
        InputManager.Clear();
        PoolManager.Clear();
        ResourceManager.Clear();
        SoundManager.Clear();
        UIManager.Clear();
    }
}
