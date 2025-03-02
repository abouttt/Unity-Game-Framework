using UnityEngine;

public static class Extension
{
    public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
    {
        return Util.GetOrAddComponent<T>(gameObject);
    }

    public static Transform FindChild(this Transform parent, string name, bool recursive = false)
    {
        return Util.FindChild(parent, name, recursive);
    }

    public static Transform FindChildWithTag(this Transform parent, string tag, bool recursive = false)
    {
        return Util.FindChildWithTag(parent, tag, recursive);
    }

    public static bool IsInLayerMask(this GameObject gameObject, LayerMask layerMask)
    {
        return Util.IsInLayerMask(gameObject, layerMask);
    }

    public static string ToSnake(this string str)
    {
        return Util.ToSnake(str);
    }
}
