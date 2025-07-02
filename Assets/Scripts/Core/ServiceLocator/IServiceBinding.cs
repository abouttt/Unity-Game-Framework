using UnityEngine;

namespace GameFramework
{
    interface IServiceBinding
    {
        ServiceLifetime Lifetime { get; }
        object GetInstance();
    }
}
