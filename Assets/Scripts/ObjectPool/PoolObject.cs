using UnityEngine;

namespace GameFramework
{
    public abstract class PoolObject : MonoBehaviour, IPoolable
    {
        public string PoolKey { get; set; }
        public bool IsUsing { get; set; }

        public abstract void OnCreate();
        public abstract void OnGetFromPool();
        public abstract void OnReturnToPool();
        public abstract void OnRelease();
    }
}
