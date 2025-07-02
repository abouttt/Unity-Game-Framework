using System;
using UnityEngine;

namespace GameFramework
{
    public class ServiceBinding<T> : IServiceBinding where T : IService
    {
        public ServiceLifetime Lifetime { get; }

        private readonly Func<T> _factory;
        private T _instance;

        public ServiceBinding(Func<T> factory, ServiceLifetime lifetime)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            Lifetime = lifetime;

            if (Lifetime == ServiceLifetime.Singleton)
            {
                _instance = _factory();
            }
        }

        public object GetInstance()
        {
            return Lifetime switch
            {
                ServiceLifetime.Singleton => _instance,
                ServiceLifetime.Lazy => _instance ??= _factory(),
                ServiceLifetime.Transient => _factory(),
                _ => throw new NotSupportedException($"Lifetime {Lifetime} is not supported.")
            };
        }
    }
}
