using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public abstract class BaseScene : MonoBehaviour
{
    private void Awake()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (ResourceManager.Count == 0 &&
            SceneSettings.Instance[sceneName].ReloadSceneWhenNoResources)
        {
            SceneLoader.ReadyToLoad(sceneName);
        }
        else
        {
            Init();
        }
    }

    protected virtual void Init()
    {
        if (FindAnyObjectByType<EventSystem>() == null)
        {
            ResourceManager.InstantiateAsync("EventSystem");
        }
    }
}
