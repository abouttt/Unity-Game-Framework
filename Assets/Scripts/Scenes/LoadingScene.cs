using System;
using System.Collections;
using UnityEngine;

public class LoadingScene : BaseScene
{
    [SerializeField]
    private string _defaultSceneAddress;

    protected override void Init()
    {
        base.Init();
        Managers.Clear();
        Resources.UnloadUnusedAssets();
        GC.Collect();

        if (!Managers.Scene.IsReadyToLoad)
        {
            Managers.Scene.ReadyToLoad(_defaultSceneAddress);
        }

        Managers.Scene.ReadyToLoadComplete += () =>
        {
            StartCoroutine(LoadComplete());
        };
    }

    private void Start()
    {
        LoadResourcesByLabels(() => Managers.Scene.StartLoad());
    }

    private IEnumerator LoadComplete()
    {
        yield return YieldCache.WaitForSeconds(SceneSettings.Instance.FadeOutDuration);
        Managers.Scene.CompleteLoad();
    }

    private void LoadResourcesByLabels(Action callback = null)
    {
        var loadResourceLabels = SceneSettings.Instance[Managers.Scene.NextSceneAddress].AddressableLabels;
        if (loadResourceLabels == null || loadResourceLabels.Length == 0)
        {
            callback?.Invoke();
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
}
