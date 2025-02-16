using System;
using UnityEngine;

[DisallowMultipleComponent]
public class ObjectBinding : DataBinding
{
    public override Type BindingType => typeof(GameObject);

    protected override void Setup()
    {
        Target = gameObject;
    }
}
