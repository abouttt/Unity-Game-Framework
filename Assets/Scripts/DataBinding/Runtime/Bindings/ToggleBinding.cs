using System;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Toggle))]
public class ToggleBinding : DataBinding
{
    public override Type BindingType => typeof(Toggle);
}
