using UnityEngine;
using CookieGame.Core;
using CookieGame.Data;
using CookieGame.Patterns;

namespace CookieGame.VFX
{
    /// <summary>
    /// VFX Manager - handles all particle effects and visual feedback
    /// Uses object pooling for performance
    /// Follows Single Responsibility Principle
    /// </summary>
    public class VFXManager : MonoBehaviour
    {
        [Header("Database")]
        [SerializeField] private VFXDatabase _vfxDatabase;

        private EventManager _eventManager;
        private PoolManager _poolManager;

        private void Start()
        {
            _eventManager = ServiceLocator.Instance.Get<EventManager>();
            _poolManager = ServiceLocator.Instance.Get<PoolManager>();

            // Subscribe to VFX request events
            _eventManager?.Subscribe<VFXRequestEvent>(OnVFXRequest);

            // Initialize VFX database
            if (_vfxDatabase != null)
            {
                _vfxDatabase.Initialize();
                CreateVFXPools();
            }
        }

        private void CreateVFXPools()
        {
            foreach (var vfxEntry in _vfxDatabase.particleEffects)
            {
                if (vfxEntry.prefab != null)
                {
                    // Try to get or add the required component
                    var prefabComponent = vfxEntry.prefab.GetComponent<VFXInstance>();
                    if (prefabComponent == null)
                    {
                        // If the prefab doesn't have the component, add it temporarily for pooling
                        prefabComponent = vfxEntry.prefab.AddComponent<VFXInstance>();
                    }

                    _poolManager?.CreatePool(vfxEntry.key, prefabComponent, 5);
                }
            }

            Debug.Log("VFX pools created");
        }

        private void OnVFXRequest(VFXRequestEvent eventData)
        {
            PlayVFX(eventData.vfxName, eventData.position);
        }

        /// <summary>
        /// Plays a VFX at the specified position
        /// </summary>
        public void PlayVFX(string vfxName, Vector3 position, Transform parent = null)
        {
            if (_vfxDatabase == null)
            {
                Debug.LogWarning("VFX Database not assigned!");
                return;
            }

            var vfxEntry = _vfxDatabase.GetVFX(vfxName);
            if (vfxEntry != null && vfxEntry.prefab != null)
            {
                VFXInstance vfx = _poolManager.Get<VFXInstance>(vfxEntry.key);
                if (vfx != null)
                {
                    vfx.transform.position = position;
                    vfx.transform.SetParent(parent);
                    vfx.Play(vfxEntry.duration, vfxEntry.autoDestroy);

                    Debug.Log($"Playing VFX: {vfxName} at {position}");
                }
            }
            else
            {
                Debug.LogWarning($"VFX not found: {vfxName}");
            }
        }

        /// <summary>
        /// Plays a VFX and returns the instance for further control
        /// </summary>
        public VFXInstance PlayVFXWithReturn(string vfxName, Vector3 position, Transform parent = null)
        {
            if (_vfxDatabase == null) return null;

            var vfxEntry = _vfxDatabase.GetVFX(vfxName);
            if (vfxEntry != null && vfxEntry.prefab != null)
            {
                VFXInstance vfx = _poolManager.Get<VFXInstance>(vfxEntry.key);
                if (vfx != null)
                {
                    vfx.transform.position = position;
                    vfx.transform.SetParent(parent);
                    vfx.Play(vfxEntry.duration, vfxEntry.autoDestroy);
                    return vfx;
                }
            }

            return null;
        }

        /// <summary>
        /// Stops and returns VFX to pool
        /// </summary>
        public void StopVFX(VFXInstance vfx, string poolKey)
        {
            if (vfx != null)
            {
                vfx.Stop();
                _poolManager?.Return(poolKey, vfx);
            }
        }

        private void OnDestroy()
        {
            _eventManager?.Unsubscribe<VFXRequestEvent>(OnVFXRequest);
        }
    }
}
