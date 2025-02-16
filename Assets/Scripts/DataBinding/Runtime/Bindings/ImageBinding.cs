using System;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Image))]
public class ImageBinding : DataBinding
{
    public override Type BindingType => typeof(Image);
}
