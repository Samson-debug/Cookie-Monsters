using UnityEngine;
using System.Collections.Generic;

namespace CookieGame.Data
{
    /// <summary>
    /// Database for all VFX prefabs in the game
    /// Centralizes visual effects management
    /// </summary>
    [CreateAssetMenu(fileName = "VFXDatabase", menuName = "Cookie Game/VFX Database")]
    public class VFXDatabase : ScriptableObject
    {
        [System.Serializable]
        public class VFXEntry
        {
            public string key;
            public GameObject prefab;
            public float duration = 2f;
            public bool autoDestroy = true;
        }

        [Header("Particle Effects")]
        public List<VFXEntry> particleEffects = new List<VFXEntry>();

        private Dictionary<string, VFXEntry> _vfxLookup;

        /// <summary>
        /// Initializes VFX lookup dictionary
        /// </summary>
        public void Initialize()
        {
            _vfxLookup = new Dictionary<string, VFXEntry>();

            foreach (var entry in particleEffects)
            {
                if (!string.IsNullOrEmpty(entry.key))
                {
                    _vfxLookup[entry.key] = entry;
                }
            }

            Debug.Log($"VFX Database initialized: {_vfxLookup.Count} effects");
        }

        /// <summary>
        /// Gets VFX entry by key
        /// </summary>
        public VFXEntry GetVFX(string key)
        {
            if (_vfxLookup == null) Initialize();
            return _vfxLookup.TryGetValue(key, out var entry) ? entry : null;
        }

        private void OnEnable()
        {
            Initialize();
        }
    }
}
