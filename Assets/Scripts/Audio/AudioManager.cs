using UnityEngine;
using System.Collections.Generic;
using CookieGame.Core;
using CookieGame.Data;

namespace CookieGame.Audio
{
    /// <summary>
    /// Audio Manager - handles all sound and music in the game
    /// Follows Single Responsibility Principle
    /// Uses object pooling for AudioSource components
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        [Header("Audio Settings")]
        [SerializeField] private AudioDatabase _audioDatabase;
        [SerializeField] private int _maxSimultaneousSFX = 10;

        [Header("Audio Sources")]
        [SerializeField] private AudioSource _musicSource;

        private GameConfig _config;
        private EventManager _eventManager;
        private Queue<AudioSource> _sfxPool;
        private List<AudioSource> _activeSFX;

        private void Awake()
        {
            InitializeAudioSources();
        }

        private void Start()
        {
            _config = Resources.Load<GameConfig>("GameConfig");
            if (_config == null)
            {
                _config = ScriptableObject.CreateInstance<GameConfig>();
            }

            _eventManager = ServiceLocator.Instance.Get<EventManager>();

            // Subscribe to audio request events
            _eventManager?.Subscribe<SoundRequestEvent>(OnSoundRequest);

            // Initialize audio database
            if (_audioDatabase != null)
            {
                _audioDatabase.Initialize();
            }

            // Apply volume settings
            ApplyVolumeSettings();
        }

        private void InitializeAudioSources()
        {
            // Create music source if not assigned
            if (_musicSource == null)
            {
                GameObject musicObj = new GameObject("MusicSource");
                musicObj.transform.SetParent(transform);
                _musicSource = musicObj.AddComponent<AudioSource>();
                _musicSource.loop = true;
                _musicSource.playOnAwake = false;
            }

            // Create SFX pool
            _sfxPool = new Queue<AudioSource>();
            _activeSFX = new List<AudioSource>();

            for (int i = 0; i < _maxSimultaneousSFX; i++)
            {
                CreateSFXSource();
            }
        }

        private AudioSource CreateSFXSource()
        {
            GameObject sfxObj = new GameObject($"SFX_Source_{_sfxPool.Count}");
            sfxObj.transform.SetParent(transform);
            AudioSource source = sfxObj.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            _sfxPool.Enqueue(source);
            return source;
        }

        private void ApplyVolumeSettings()
        {
            if (_config != null)
            {
                AudioListener.volume = _config.masterVolume;
                if (_musicSource != null)
                {
                    _musicSource.volume = _config.musicVolume;
                }
            }
        }

        private void OnSoundRequest(SoundRequestEvent eventData)
        {
            if (eventData.isMusic)
            {
                PlayMusic(eventData.soundName);
            }
            else
            {
                PlaySFX(eventData.soundName);
            }
        }

        /// <summary>
        /// Plays background music
        /// </summary>
        public void PlayMusic(string musicName, bool loop = true)
        {
            if (_audioDatabase == null)
            {
                Debug.LogWarning("Audio Database not assigned!");
                return;
            }

            var musicEntry = _audioDatabase.GetMusic(musicName);
            if (musicEntry != null && musicEntry.clip != null)
            {
                _musicSource.clip = musicEntry.clip;
                _musicSource.volume = musicEntry.volume * _config.musicVolume;
                _musicSource.loop = loop;
                _musicSource.Play();

                Debug.Log($"Playing music: {musicName}");
            }
            else
            {
                Debug.LogWarning($"Music not found: {musicName}");
            }
        }

        /// <summary>
        /// Stops currently playing music
        /// </summary>
        public void StopMusic(float fadeOutTime = 0.5f)
        {
            if (_musicSource.isPlaying)
            {
                if (fadeOutTime > 0f)
                {
                    // Fade out using coroutine
                    StartCoroutine(FadeOutMusic(fadeOutTime));
                }
                else
                {
                    _musicSource.Stop();
                }
            }
        }

        private System.Collections.IEnumerator FadeOutMusic(float duration)
        {
            float startVolume = _musicSource.volume;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                _musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
                yield return null;
            }

            _musicSource.Stop();
            _musicSource.volume = startVolume;
        }

        /// <summary>
        /// Pauses music
        /// </summary>
        public void PauseMusic()
        {
            if (_musicSource.isPlaying)
            {
                _musicSource.Pause();
            }
        }

        /// <summary>
        /// Resumes music
        /// </summary>
        public void ResumeMusic()
        {
            if (!_musicSource.isPlaying)
            {
                _musicSource.UnPause();
            }
        }

        /// <summary>
        /// Plays a sound effect
        /// </summary>
        public void PlaySFX(string sfxName, float volumeMultiplier = 1f)
        {
            if (_audioDatabase == null)
            {
                Debug.LogWarning("Audio Database not assigned!");
                return;
            }

            var sfxEntry = _audioDatabase.GetSFX(sfxName);
            if (sfxEntry != null && sfxEntry.clip != null)
            {
                AudioSource source = GetAvailableSFXSource();
                if (source != null)
                {
                    source.clip = sfxEntry.clip;
                    source.volume = sfxEntry.volume * _config.sfxVolume * volumeMultiplier;
                    source.loop = sfxEntry.loop;
                    source.Play();

                    _activeSFX.Add(source);

                    // Return to pool after clip finishes
                    if (!sfxEntry.loop)
                    {
                        StartCoroutine(ReturnSFXSourceAfterDelay(source, sfxEntry.clip.length));
                    }

                    Debug.Log($"Playing SFX: {sfxName}");
                }
            }
            else
            {
                Debug.LogWarning($"SFX not found: {sfxName}");
            }
        }

        /// <summary>
        /// Plays SFX at a specific position (3D sound)
        /// </summary>
        public void PlaySFXAtPosition(string sfxName, Vector3 position, float volumeMultiplier = 1f)
        {
            if (_audioDatabase == null) return;

            var sfxEntry = _audioDatabase.GetSFX(sfxName);
            if (sfxEntry != null && sfxEntry.clip != null)
            {
                AudioSource.PlayClipAtPoint(sfxEntry.clip, position,
                    sfxEntry.volume * _config.sfxVolume * volumeMultiplier);
            }
        }

        /// <summary>
        /// Stops all sound effects
        /// </summary>
        public void StopAllSFX()
        {
            foreach (var source in _activeSFX)
            {
                if (source != null && source.isPlaying)
                {
                    source.Stop();
                }
            }

            _activeSFX.Clear();
        }

        private AudioSource GetAvailableSFXSource()
        {
            // Clean up finished sources
            _activeSFX.RemoveAll(source => source == null || !source.isPlaying);

            // Get from pool or create new if needed
            if (_sfxPool.Count > 0)
            {
                return _sfxPool.Dequeue();
            }
            else if (_activeSFX.Count < _maxSimultaneousSFX)
            {
                return CreateSFXSource();
            }

            Debug.LogWarning("Max simultaneous SFX reached!");
            return null;
        }

        private System.Collections.IEnumerator ReturnSFXSourceAfterDelay(AudioSource source, float delay)
        {
            yield return new WaitForSeconds(delay);

            if (source != null)
            {
                _activeSFX.Remove(source);
                _sfxPool.Enqueue(source);
            }
        }

        /// <summary>
        /// Sets master volume (0-1)
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            AudioListener.volume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat("MasterVolume", volume);
        }

        /// <summary>
        /// Sets music volume (0-1)
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            float clampedVolume = Mathf.Clamp01(volume);
            if (_musicSource != null)
            {
                _musicSource.volume = clampedVolume;
            }
            PlayerPrefs.SetFloat("MusicVolume", volume);
        }

        /// <summary>
        /// Sets SFX volume (0-1)
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            PlayerPrefs.SetFloat("SFXVolume", Mathf.Clamp01(volume));
        }

        private void OnDestroy()
        {
            _eventManager?.Unsubscribe<SoundRequestEvent>(OnSoundRequest);
        }
    }
}
