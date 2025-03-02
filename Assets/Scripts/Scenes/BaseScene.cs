using UnityEngine;
using UnityEngine.EventSystems;

public abstract class BaseScene : MonoBehaviour
{
    private void Awake()
    {
        string sceneName = Managers.Scene.CurrentSceneName;

        if (Managers.Resource.ResourceCount == 0 &&
            Settings.Scene[sceneName].ReloadSceneWhenNoResources)
        {
            Managers.Scene.PrepareLoad(sceneName);
        }
        else
        {
            Initialize();
        }
    }

    protected virtual void Initialize()
    {
        if (FindAnyObjectByType<EventSystem>() == null)
        {
            Managers.Resource.InstantiateAsync("EventSystem");
        }
    }
}
