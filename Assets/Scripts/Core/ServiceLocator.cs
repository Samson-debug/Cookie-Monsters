using System;
using System.Collections.Generic;
using UnityEngine;

namespace CookieGame.Core
{
    /// <summary>
    /// Service Locator pattern for dependency injection
    /// Follows Dependency Inversion Principle from SOLID
    /// </summary>
    public class ServiceLocator
    {
        private static ServiceLocator _instance;
        public static ServiceLocator Instance => _instance ??= new ServiceLocator();

        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public void Register<T>(T service) where T : class
        {
            var type = typeof(T);
            if (_services.ContainsKey(type))
            {
                Debug.LogWarning($"Service {type.Name} is already registered. Overwriting...");
                _services[type] = service;
            }
            else
            {
                _services.Add(type, service);
            }
        }

        public T Get<T>() where T : class
        {
            var type = typeof(T);
            if (_services.TryGetValue(type, out var service))
            {
                return service as T;
            }

            Debug.LogError($"Service {type.Name} not found!");
            return null;
        }

        public bool Has<T>() where T : class
        {
            return _services.ContainsKey(typeof(T));
        }

        public void Unregister<T>() where T : class
        {
            _services.Remove(typeof(T));
        }

        public void Clear()
        {
            _services.Clear();
        }
    }
}
