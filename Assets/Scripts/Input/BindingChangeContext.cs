using UnityEngine;
using UnityEngine.InputSystem;

public struct BindingChangeContext
{
    public InputActionMap ActionMap;
    public InputAction Action;
    public int BindingIndex;
    public string NewBindingPath;
}
