using System.Collections.Generic;
using UnityEngine;

namespace CookieGame.Patterns
{
    /// <summary>
    /// Object Pool pattern for performance optimization
    /// Reduces garbage collection and instantiation overhead
    /// </summary>
    public class ObjectPool<T> where T : Component
    {
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly Queue<T> _pool = new Queue<T>();
        private readonly List<T> _activeObjects = new List<T>();
        private readonly int _initialSize;

        public ObjectPool(T prefab, int initialSize = 10, Transform parent = null)
        {
            _prefab = prefab;
            _initialSize = initialSize;
            _parent = parent;
            
            InitializePool();
        }

        private void InitializePool()
        {
            for (int i = 0; i < _initialSize; i++)
            {
                CreateNewObject();
            }
        }

        private T CreateNewObject()
        {
            T obj = Object.Instantiate(_prefab, _parent);
            obj.gameObject.SetActive(false);
            _pool.Enqueue(obj);
            return obj;
        }

        public T Get()
        {
            T obj = _pool.Count > 0 ? _pool.Dequeue() : CreateNewObject();
            obj.gameObject.SetActive(true);
            _activeObjects.Add(obj);
            return obj;
        }

        public void Return(T obj)
        {
            if (obj == null) return;
            
            obj.gameObject.SetActive(false);
            _activeObjects.Remove(obj);
            _pool.Enqueue(obj);
        }

        public void ReturnAll()
        {
            for (int i = _activeObjects.Count - 1; i >= 0; i--)
            {
                Return(_activeObjects[i]);
            }
        }

        public int ActiveCount => _activeObjects.Count;
        public int PoolCount => _pool.Count;
    }

  
}
