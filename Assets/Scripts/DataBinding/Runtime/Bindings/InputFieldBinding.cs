using System;
using UnityEngine;
using TMPro;

[DisallowMultipleComponent]
[RequireComponent(typeof(TMP_InputField))]
public class InputFieldBinding : DataBinding
{
    public override Type BindingType => typeof(TMP_InputField);
}
