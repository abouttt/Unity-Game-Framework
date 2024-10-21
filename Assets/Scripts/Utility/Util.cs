using System.Linq;
using UnityEngine;

public static class Util
{
    public static T GetOrAddComponent<T>(GameObject go) where T : Component
    {
        if (go.TryGetComponent<T>(out var component))
        {
            return component;
        }

        return go.AddComponent<T>();
    }

    public static GameObject FindChild(GameObject go, string name = null, bool recursive = false)
    {
        var transform = FindChild<Transform>(go, name, recursive);
        return transform == null ? null : transform.gameObject;
    }

    public static T FindChild<T>(GameObject go, string name = null, bool recursive = false) where T : Object
    {
        if (recursive)
        {
            return go.GetComponentsInChildren<T>().FirstOrDefault(component => string.IsNullOrEmpty(name) || component.name.Equals(name));
        }
        else
        {
            for (int i = 0; i < go.transform.childCount; i++)
            {
                var transform = go.transform.GetChild(i);
                if (string.IsNullOrEmpty(name) || transform.name.Equals(name))
                {
                    if (transform.TryGetComponent<T>(out var component))
                    {
                        return component;
                    }
                }
            }
        }

        return null;
    }

    public static Transform FindChildWithTag(Transform parent, string tag)
    {
        foreach (Transform child in parent)
        {
            if (child.CompareTag(tag))
            {
                return child;
            }
        }

        return null;
    }

    public static bool IsInLayerMask(GameObject go, LayerMask layerMask)
    {
        return (layerMask.value & (1 << go.layer)) != 0;
    }
}
