using UnityEngine;

public class ReadOnlyAttribute : PropertyAttribute
{
    public readonly bool RuntimeOnly;

    public ReadOnlyAttribute(bool runtimeOnly = false)
    {
        RuntimeOnly = runtimeOnly;
    }
}
