using System;
using UnityEngine;
using TMPro;

[DisallowMultipleComponent]
[RequireComponent(typeof(TextMeshProUGUI))]
public class TextBinding : DataBinding
{
    public override Type BindingType => typeof(TextMeshProUGUI);
}
