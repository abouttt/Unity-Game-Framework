using System;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public class RectTransformBinding : DataBinding
{
    public override Type BindingType => typeof(RectTransform);
}
