using System;
using UnityEngine;

public class CustomBinding : DataBinding
{
    public string TypeName;
    public override Type BindingType => Type.GetType(TypeName);
}
