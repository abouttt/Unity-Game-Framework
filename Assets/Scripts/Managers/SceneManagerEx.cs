using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public sealed class SceneManagerEx : MonoBehaviourSingleton<SceneManagerEx>
{
    public event Action ReadyToLoadComplete;

    public string NextSceneAddress => _nextSceneAddress;
    public bool IsReadyToLoad => _isReadyToLoad;
    public bool IsLoading => _sceneHandle.IsValid() && !_sceneHandle.IsDone;
    public bool IsReadyToLoadComplete => _isReadyToLoadComplete;
    public float LoadingProgress => _loadingProgress;

    private string _nextSceneAddress;
    private bool _isReadyToLoad;
    private bool _isReadyToLoadComplete;
    private float _loadingProgress;
    private AsyncOperationHandle<SceneInstance> _sceneHandle;
    private AsyncOperationHandle<SceneInstance> _unloadHandle;

    protected override void Init()
    {
        base.Init();
        Addressables.InitializeAsync();
    }

    public void ReadyToLoad(string sceneAddress)
    {
        if (IsLoading)
        {
            Debug.LogWarning($"[SceneManagerEx.ReadyToLoad] Already loading : {_nextSceneAddress}");
            return;
        }

        if (string.IsNullOrEmpty(sceneAddress))
        {
            Debug.LogWarning("[SceneManagerEx.ReadyToLoad] The scene address is empty");
            return;
        }

        if (_isReadyToLoadComplete)
        {
            ClearStatus();
            UnloadScene(_sceneHandle);
        }

        _nextSceneAddress = sceneAddress;
        _isReadyToLoad = true;

        if (!SceneManager.GetActiveScene().name.Equals("LoadingScene"))
        {
            SceneManager.LoadScene("LoadingScene");
        }
    }

    public void StartLoad()
    {
        if (IsLoading)
        {
            Debug.LogWarning($"[SceneManagerEx.StartLoad] Already loading : {_nextSceneAddress}");
            return;
        }

        if (_isReadyToLoad)
        {
            _sceneHandle = Addressables.LoadSceneAsync(_nextSceneAddress, LoadSceneMode.Single, false);
            StartCoroutine(UpdateLoadingProgress(_sceneHandle));
        }
        else
        {
            Debug.LogWarning("[SceneManagerEx.StartLoad] Not ready to load");
        }
    }

    public void CompleteLoad()
    {
        if (_isReadyToLoadComplete)
        {
            ClearStatus();
            _sceneHandle.Result.ActivateAsync();
        }
        else
        {
            Debug.LogWarning("[SceneManagerEx.CompleteLoad] Not ready to complete");
        }
    }

    private IEnumerator UpdateLoadingProgress(AsyncOperationHandle<SceneInstance> handle)
    {
        if (!handle.IsValid())
        {
            yield break;
        }

        float timer = 0f;

        while (!handle.IsDone)
        {
            yield return null;

            timer += Time.deltaTime;

            if (handle.PercentComplete < 0.9f)
            {
                _loadingProgress = Mathf.Lerp(_loadingProgress, handle.PercentComplete, timer);
            }
        }

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            timer = 0f;

            while (true)
            {
                yield return null;

                timer += Time.deltaTime;

                _loadingProgress = Mathf.Lerp(_loadingProgress, 1f, timer);
                if (_loadingProgress >= 1f)
                {
                    _isReadyToLoadComplete = true;
                    ReadyToLoadComplete?.Invoke();
                    ReadyToLoadComplete = null;
                    yield break;
                }
            }
        }
        else
        {
            Debug.LogWarning($"[SceneManagerEx] {_nextSceneAddress} Scene Loading failed");
        }
    }

    private void UnloadScene(AsyncOperationHandle<SceneInstance> handle)
    {
        if (_unloadHandle.IsValid() && !_unloadHandle.IsDone)
        {
            Debug.LogWarning($"[SceneManagerEx.UnloadScene] Already unloading : {_unloadHandle.Result.Scene.name}");
            return;
        }

        if (handle.IsValid())
        {
            _unloadHandle = Addressables.UnloadSceneAsync(handle);
            _unloadHandle.Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    Resources.UnloadUnusedAssets();
                    Debug.Log($"[SceneManagerEx.UnloadScene] Scene unload succeeded : {_unloadHandle.Result.Scene.name}");
                }
                else
                {
                    Debug.LogWarning($"[SceneManagerEx.UnloadScene] Scene unload failed : {_unloadHandle.Result.Scene.name}");
                }
            };
        }
    }

    private void ClearStatus()
    {
        _nextSceneAddress = null;
        _isReadyToLoad = false;
        _isReadyToLoadComplete = false;
        _loadingProgress = 0f;
    }
}
