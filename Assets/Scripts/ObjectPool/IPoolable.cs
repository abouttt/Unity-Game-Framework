using UnityEngine;

namespace GameFramework
{
    public interface IPoolable
    {
        public string PoolKey { get; }
        public bool IsUsing { get; }

        void OnCreate();
        void OnGetFromPool();
        void OnReturnToPool();
        void OnRelease();
    }
}
