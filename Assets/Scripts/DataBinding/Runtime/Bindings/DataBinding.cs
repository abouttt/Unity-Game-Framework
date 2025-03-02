using System;
using UnityEngine;
using Object = UnityEngine.Object;

public abstract class DataBinding : MonoBehaviour
{
    public Object Target;
    public string DataID;
    public bool AutoRefreshDataID = true;

    public abstract Type BindingType { get; }

    private void Reset()
    {
        DataID = gameObject.name;
        Setup();
    }

    protected virtual void Setup()
    {
        FindTarget(BindingType);
    }

    protected void FindTarget(Type type)
    {
        if (TryGetComponent(type, out var component))
        {
            Target = component;
        }
        else
        {
            Debug.LogError($"[{BindingType}Binding] Required component {type.Name} is missing on {gameObject.name}");
        }
    }
}
