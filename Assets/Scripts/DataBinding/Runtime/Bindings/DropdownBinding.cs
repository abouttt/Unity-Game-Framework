using System;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Dropdown))]
public class DropdownBinding : DataBinding
{
    public override Type BindingType => typeof(Dropdown);
}
