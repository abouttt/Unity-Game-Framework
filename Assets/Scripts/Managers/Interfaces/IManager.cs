using UnityEngine;

public interface IManager
{
    void Initialize();
    void Clear();

    protected static Transform CreateRoot(string name)
    {
        var rootGameObject = new GameObject(name);
        Object.DontDestroyOnLoad(rootGameObject);
        return rootGameObject.transform;
    }
}
