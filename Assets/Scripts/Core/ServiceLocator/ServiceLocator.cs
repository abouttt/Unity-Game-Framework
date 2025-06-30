using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework
{
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> s_bindings = new();
        private static readonly Dictionary<Type, Func<object>> s_binders = new();
        private static readonly Dictionary<Type, Action> s_unbinders = new();

        public static void Bind<T>(Func<T> factory, ServiceLifetime lifetime = ServiceLifetime.Singleton) where T : class, IService, new()
        {
            var type = typeof(T);
            if (s_bindings.ContainsKey(type))
            {
                Debug.Log($"[ServiceLocator] {type} is already bound");
                return;
            }

            var binding = new ServiceBinding<T>(factory, lifetime);
            s_bindings[type] = binding;
            s_binders[type] = () => binding.GetInstance();
            s_unbinders[type] = () => binding.Unbind();
        }

        public static void Bind<TInterface, TImpl>(Func<TImpl> factory, ServiceLifetime lifetime = ServiceLifetime.Singleton)
            where TInterface : class, IService
            where TImpl : class, TInterface, IService, new()
        {
            var type = typeof(TInterface);
            if (s_bindings.ContainsKey(type))
            {
                Debug.Log($"[ServiceLocator] {type} is already bound");
                return;
            }

            var binding = new ServiceBinding<TImpl>(factory, lifetime);
            s_bindings[type] = binding;
            s_binders[type] = () => binding.GetInstance();
            s_unbinders[type] = () => binding.Unbind();
        }

        public static T Get<T>() where T : class, IService
        {
            var type = typeof(T);
            if (s_binders.TryGetValue(type, out var bind))
            {
                return bind() as T;
            }

            Debug.LogWarning($"[ServiceLocator] {type} is not bound");
            return default;
        }

        public static void Unbind<T>() where T : IService, new()
        {
            var type = typeof(T);
            if (s_unbinders.TryGetValue(type, out var unbind))
            {
                unbind?.Invoke();
            }

            s_bindings.Remove(type);
            s_binders.Remove(type);
            s_unbinders.Remove(type);
        }

        public static bool IsBound<T>()
        {
            return s_bindings.ContainsKey(typeof(T));
        }

        public static void Clear()
        {
            foreach (var unbind in s_unbinders.Values)
            {
                unbind?.Invoke();
            }

            s_bindings.Clear();
            s_binders.Clear();
            s_unbinders.Clear();
        }
    }
}
