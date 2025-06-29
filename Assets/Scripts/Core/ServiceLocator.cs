using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework.Core
{
    public class ServiceLocator : MonoSingleton<ServiceLocator>
    {
        private readonly Dictionary<Type, object> _services = new();

        public void Register<T>(T service) where T : class
        {
            var type = typeof(T);
            if (_services.ContainsKey(type))
            {
                Debug.LogWarning($"[ServiceLocator] Service already registered: {type.Name}");
                return;
            }

            _services[type] = service;
        }

        public void Register<TInterface, TConcrete>()
            where TInterface : class
            where TConcrete : class, TInterface, new()
        {
            Register<TInterface>(new TConcrete());
        }

        public void Unregister<T>() where T : class
        {
            var type = typeof(T);
            _services.Remove(type);
        }

        public T Resolve<T>() where T : class
        {
            var type = typeof(T);
            if (_services.TryGetValue(type, out var service))
            {
                return service as T;
            }

            Debug.LogError($"[ServiceLocator] Service not found: {type.Name}");
            return null;
        }

        public bool IsRegistered<T>() where T : class
        {
            return _services.ContainsKey(typeof(T));
        }

        protected override void OnDestroy()
        {
            _services.Clear();
        }
    }
}
