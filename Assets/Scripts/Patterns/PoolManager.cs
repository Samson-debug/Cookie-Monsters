using UnityEngine;
using System.Collections.Generic;
namespace CookieGame.Patterns
{
    /// <summary>
    /// Manager for multiple object pools
    /// </summary>
    public class PoolManager : MonoBehaviour
    {
        private readonly Dictionary<string, object> _pools = new Dictionary<string, object>();

        public void CreatePool<T>(string key, T prefab, int initialSize = 10) where T : Component
        {
            if (!_pools.ContainsKey(key))
            {
                var pool = new ObjectPool<T>(prefab, initialSize, transform);
                _pools[key] = pool;
                Debug.Log($"Created pool for {key} with initial size {initialSize}");
            }
        }

        public T Get<T>(string key) where T : Component
        {
            if (_pools.TryGetValue(key, out var pool))
            {
                return (pool as ObjectPool<T>)?.Get();
            }

            Debug.LogError($"Pool {key} not found!");
            return null;
        }

        public void Return<T>(string key, T obj) where T : Component
        {
            if (_pools.TryGetValue(key, out var pool))
            {
                (pool as ObjectPool<T>)?.Return(obj);
            }
        }

        public void ReturnAll<T>(string key) where T : Component
        {
            if (_pools.TryGetValue(key, out var pool))
            {
                (pool as ObjectPool<T>)?.ReturnAll();
            }
        }
    }
}