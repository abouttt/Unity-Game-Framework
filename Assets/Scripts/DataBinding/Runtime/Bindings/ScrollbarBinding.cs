using System;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Scrollbar))]
public class ScrollbarBinding : DataBinding
{
    public override Type BindingType => typeof(Scrollbar);
}
