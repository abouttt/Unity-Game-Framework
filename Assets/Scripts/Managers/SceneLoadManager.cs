using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class SceneLoadManager : IManager
{
    public event Action Loaded;

    public string CurrentSceneName => SceneManager.GetActiveScene().name;
    public string NextSceneName { get; private set; }
    public bool IsPrepare { get; private set; }
    public bool IsLoading { get; private set; }
    public bool IsLoaded { get; private set; }
    public float Progress { get; private set; }

    private AsyncOperation _loadingOp;

    public void Initialize()
    {
        ClearStatus();
    }

    public void PrepareLoad(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning($"[SceneLoader] The scene name for preparing to load is null");
            return;
        }

        if (IsLoading || IsLoaded)
        {
            Debug.LogWarning($"[SceneLoader] Already in loading or loaded: {NextSceneName}");
            return;
        }

        NextSceneName = sceneName;
        IsPrepare = true;

        var loadingSceneName = Settings.Scene.LoadingSceneName;
        if (CurrentSceneName != loadingSceneName)
        {
            SceneManager.LoadScene(loadingSceneName);
        }
    }

    public async void StartLoad()
    {
        if (!IsPrepare)
        {
            Debug.LogWarning("[SceneLoader] Before you start loading, prepare to load");
            return;
        }

        if (IsLoading)
        {
            Debug.LogWarning($"[SceneLoader] Already in loading: {NextSceneName}");
            return;
        }

        await LoadSceneAsync();
    }

    public void SwitchLoadedScene()
    {
        if (!IsLoaded)
        {
            Debug.LogWarning("[SceneLoader] Not loaded");
            return;
        }

        _loadingOp.allowSceneActivation = true;
        ClearStatus();
    }

    public void Clear()
    {

    }

    private async Awaitable LoadSceneAsync()
    {
        try
        {
            _loadingOp = SceneManager.LoadSceneAsync(NextSceneName, LoadSceneMode.Single);
            _loadingOp.allowSceneActivation = false;
            IsLoading = true;

            float timer = 0f;
            float targetProgress = 0.9f;
            float delay = Settings.Scene.LoadingDelay;

            while (!_loadingOp.isDone)
            {
                await Awaitable.NextFrameAsync();

                if (_loadingOp.progress < targetProgress)
                {
                    Progress = _loadingOp.progress;
                }
                else
                {
                    timer += Time.unscaledDeltaTime;
                    float transitionProgress = Mathf.Clamp01(timer / delay);
                    Progress = Mathf.Lerp(Progress, 1f, transitionProgress);

                    if (Progress >= 1f)
                    {
                        IsLoading = false;
                        IsLoaded = true;
                        Loaded?.Invoke();
                        break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SceneLoader] Error loading scene: {ex.Message}");
            ClearStatus();
        }
    }

    private void ClearStatus()
    {
        NextSceneName = null;
        IsPrepare = false;
        IsLoaded = false;
        Progress = 0f;
        _loadingOp = null;
    }
}
