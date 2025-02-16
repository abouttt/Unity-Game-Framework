using System;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Button))]
public class ButtonBinding : DataBinding
{
    public override Type BindingType => typeof(Button);
}
