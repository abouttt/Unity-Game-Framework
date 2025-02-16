using System;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Slider))]
public class SliderBinding : DataBinding
{
    public override Type BindingType => typeof(Slider);
}
