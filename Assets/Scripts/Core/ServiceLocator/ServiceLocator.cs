using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework
{
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, IServiceBinding> _bindings = new();

        public static void Bind<T>(Func<T> factory, ServiceLifetime lifetime = ServiceLifetime.Singleton) where T : class, IService
        {
            BindInternal(typeof(T), factory, lifetime);
        }

        public static void Bind<TInterface, TImpl>(Func<TImpl> factory, ServiceLifetime lifetime = ServiceLifetime.Singleton)
            where TInterface : class, IService
            where TImpl : class, TInterface, IService, new()
        {
            BindInternal(typeof(TInterface), factory, lifetime);
        }

        public static void BindInstance<T>(T instance) where T : class, IService
        {
            BindInternal(typeof(T), () => instance, ServiceLifetime.Singleton);
        }

        public static T Get<T>() where T : class, IService
        {
            var type = typeof(T);
            if (_bindings.TryGetValue(type, out var binding))
            {
                return binding.GetInstance() as T;
            }

            Debug.LogWarning($"[ServiceLocator] {type} is not bound.");
            return null;
        }

        public static void Unbind<T>() where T : IService, new()
        {
            _bindings.Remove(typeof(T));
        }

        public static bool IsBound<T>()
        {
            return _bindings.ContainsKey(typeof(T));
        }

        public static void Clear()
        {
            _bindings.Clear();
        }

        private static void BindInternal<T>(Type type, Func<T> factory, ServiceLifetime lifetime) where T : class, IService
        {
            if (_bindings.ContainsKey(type))
            {
                Debug.LogWarning($"[ServiceLocator] Service of type {type.Name} is already bound.");
                return;
            }

            var binding = new ServiceBinding<T>(factory, lifetime);
            _bindings[type] = binding;
        }
    }
}
