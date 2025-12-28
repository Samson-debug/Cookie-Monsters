using UnityEngine;
using System.Collections.Generic;

namespace CookieGame.Data
{
    /// <summary>
    /// Database for all audio clips in the game
    /// Implements a clean lookup system for audio management
    /// </summary>
    [CreateAssetMenu(fileName = "AudioDatabase", menuName = "Cookie Game/Audio Database")]
    public class AudioDatabase : ScriptableObject
    {
        [System.Serializable]
        public class AudioEntry
        {
            public string key;
            public AudioClip clip;
            [Range(0f, 1f)]
            public float volume = 1f;
            public bool loop = false;
        }

        [Header("Music")]
        public List<AudioEntry> musicTracks = new List<AudioEntry>();

        [Header("Sound Effects")]
        public List<AudioEntry> soundEffects = new List<AudioEntry>();

        private Dictionary<string, AudioEntry> _musicLookup;
        private Dictionary<string, AudioEntry> _sfxLookup;

        /// <summary>
        /// Initializes lookup dictionaries
        /// </summary>
        public void Initialize()
        {
            _musicLookup = new Dictionary<string, AudioEntry>();
            _sfxLookup = new Dictionary<string, AudioEntry>();

            foreach (var entry in musicTracks)
            {
                if (!string.IsNullOrEmpty(entry.key))
                {
                    _musicLookup[entry.key] = entry;
                }
            }

            foreach (var entry in soundEffects)
            {
                if (!string.IsNullOrEmpty(entry.key))
                {
                    _sfxLookup[entry.key] = entry;
                }
            }

            Debug.Log($"Audio Database initialized: {_musicLookup.Count} music tracks, {_sfxLookup.Count} SFX");
        }

        /// <summary>
        /// Gets music entry by key
        /// </summary>
        public AudioEntry GetMusic(string key)
        {
            if (_musicLookup == null) Initialize();
            return _musicLookup.TryGetValue(key, out var entry) ? entry : null;
        }

        /// <summary>
        /// Gets SFX entry by key
        /// </summary>
        public AudioEntry GetSFX(string key)
        {
            if (_sfxLookup == null) Initialize();
            return _sfxLookup.TryGetValue(key, out var entry) ? entry : null;
        }

        private void OnEnable()
        {
            Initialize();
        }
    }
}
