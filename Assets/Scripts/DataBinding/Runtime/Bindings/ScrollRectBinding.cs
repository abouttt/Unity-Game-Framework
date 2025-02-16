using System;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(ScrollRect))]
public class ScrollRectBinding : DataBinding
{
    public override Type BindingType => typeof(ScrollRect);
}
