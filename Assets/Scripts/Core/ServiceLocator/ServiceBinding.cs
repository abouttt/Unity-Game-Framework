using System;
using UnityEngine;

namespace GameFramework
{
    public class ServiceBinding<T> where T : IService, new()
    {
        private readonly ServiceLifetime _lifetime;
        private readonly Func<T> _factory;
        private T _instance;

        public ServiceBinding(Func<T> factory, ServiceLifetime lifetime)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _lifetime = lifetime;

            if (_lifetime == ServiceLifetime.Singleton)
            {
                _instance = CreateInstance();
            }
        }

        public T GetInstance()
        {
            return _lifetime switch
            {
                ServiceLifetime.Singleton => _instance,
                ServiceLifetime.Lazy => _instance ??= CreateInstance(),
                ServiceLifetime.Transient => CreateInstance(),
                _ => throw new NotSupportedException()
            };
        }

        public void Unbind()
        {
            _instance?.OnUnbind();
        }

        private T CreateInstance()
        {
            var instance = _factory();
            instance.OnBind();
            return instance;
        }
    }
}
