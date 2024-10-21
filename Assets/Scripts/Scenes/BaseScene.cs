using UnityEngine;
using UnityEngine.EventSystems;

public abstract class BaseScene : MonoBehaviour
{
    [field: SerializeField]
    public string SceneAddress { get; private set; }

    private void Awake()
    {
        if (Managers.Resource.Count == 0 &&
            SceneSettings.Instance[SceneAddress].ReloadSceneWhenNoResources)
        {
            Managers.Scene.ReadyToLoad(SceneAddress);
        }
        else
        {
            Init();
        }
    }

    protected virtual void Init()
    {
        Managers.Init();

        if (FindAnyObjectByType<EventSystem>() == null)
        {
            Managers.Resource.InstantiateAsync("EventSystem.prefab");
        }
    }
}
