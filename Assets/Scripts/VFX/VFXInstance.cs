using UnityEngine;
using System.Collections;

namespace CookieGame.VFX
{
    /// <summary>
    /// VFX Instance wrapper for particle systems
    /// Handles lifecycle and pooling integration
    /// </summary>
    public class VFXInstance : MonoBehaviour
    {
        private ParticleSystem[] _particleSystems;
        private Animator _animator;
        private float _duration;
        private bool _isPlaying;

        private void Awake()
        {
            _particleSystems = GetComponentsInChildren<ParticleSystem>();
            _animator = GetComponent<Animator>();
        }

        /// <summary>
        /// Plays the VFX effect
        /// </summary>
        public void Play(float duration, bool autoReturn = true)
        {
            _duration = duration;
            _isPlaying = true;

            // Play all particle systems
            foreach (var ps in _particleSystems)
            {
                if (ps != null)
                {
                    ps.Clear();
                    ps.Play();
                }
            }

            // Play animator if available
            if (_animator != null)
            {
                _animator.SetTrigger("Play");
            }

            // Auto-return to pool after duration
            if (autoReturn)
            {
                StartCoroutine(AutoReturnToPool());
            }
        }

        /// <summary>
        /// Stops the VFX effect
        /// </summary>
        public void Stop()
        {
            _isPlaying = false;

            // Stop all particle systems
            foreach (var ps in _particleSystems)
            {
                if (ps != null)
                {
                    ps.Stop();
                }
            }

            StopAllCoroutines();
        }

        private IEnumerator AutoReturnToPool()
        {
            yield return new WaitForSeconds(_duration);

            Stop();
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Resets VFX state when retrieved from pool
        /// </summary>
        public void ResetState()
        {
            Stop();

            foreach (var ps in _particleSystems)
            {
                if (ps != null)
                {
                    ps.Clear();
                }
            }
        }

        public bool IsPlaying() => _isPlaying;
    }
}
