using UnityEngine;
using CookieGame.Core;
using CookieGame.Audio;
using CookieGame.UI;

namespace CookieGame.Gameplay
{
    /// <summary>
    /// GDD V2.0: Handles remainder error detection and feedback
    /// When player tries to distribute but doesn't have enough cookies for all monsters
    /// </summary>
    public class RemainderErrorHandler : MonoBehaviour
    {
        [Header("Audio")]
        [SerializeField] private string _errorSoundName = "Error";

        [Header("Visual Feedback")]
        [SerializeField] private GameObject _errorPopupPrefab;
        [SerializeField] private Transform _errorPopupSpawnPoint;
        [SerializeField] private float _errorPopupDuration = 2f;

        private EventManager _eventManager;
        private AudioManager _audioManager;
        private UIManager _uiManager;

        private void Start()
        {
            _eventManager = ServiceLocator.Instance.Get<EventManager>();
            _audioManager = ServiceLocator.Instance.Get<AudioManager>();
            _uiManager = ServiceLocator.Instance.Get<UIManager>();

            // Subscribe to remainder error event
            _eventManager?.Subscribe<RemainderErrorEvent>(OnRemainderError);
        }

        /// <summary>
        /// Called when player tries to distribute but has remainder < divisor
        /// </summary>
        private void OnRemainderError(RemainderErrorEvent evt)
        {
            Debug.LogWarning($"Remainder Error: {evt.remainingCookies} cookies left, need {evt.divisor} for even distribution");

            // Play error sound
            PlayErrorSound();

            // Show visual feedback
            ShowErrorFeedback(evt.remainingCookies, evt.divisor);

            // Show error message popup
            ShowErrorPopup(evt.remainingCookies, evt.divisor);
        }

        /// <summary>
        /// Plays error sound effect
        /// </summary>
        private void PlayErrorSound()
        {
            if (_audioManager != null && !string.IsNullOrEmpty(_errorSoundName))
            {
                _audioManager.PlaySFX(_errorSoundName);
            }
        }

        /// <summary>
        /// Shows visual error feedback (shake, color change, etc.)
        /// </summary>
        private void ShowErrorFeedback(int remainingCookies, int divisor)
        {
            // Shake camera slightly
            if (Camera.main != null)
            {
#if DOTWEEN_ENABLED
                Camera.main.transform.DOShakePosition(0.3f, 0.1f, 10);
#else
                Debug.Log("Remainder error visual feedback (DOTween not enabled)");
#endif
            }

            // Flash screen or show error indicator
            // This can be expanded with particle effects or screen flash
        }

        /// <summary>
        /// Shows error popup message to inform the player
        /// </summary>
        private void ShowErrorPopup(int remainingCookies, int divisor)
        {
            if (_errorPopupPrefab != null && _errorPopupSpawnPoint != null)
            {
                // Instantiate error popup
                GameObject popup = Instantiate(_errorPopupPrefab, _errorPopupSpawnPoint.position, Quaternion.identity);

                // Set error message (assumes popup has TextMeshProUGUI component)
                var textComponent = popup.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (textComponent != null)
                {
                    textComponent.text = $"Not enough cookies!\n{remainingCookies} left, need {divisor} for all monsters";
                }

                // Auto-destroy after duration
                Destroy(popup, _errorPopupDuration);
            }
            else
            {
                // Fallback: Log to console
                Debug.Log($"ERROR: Not enough cookies! {remainingCookies} left, need {divisor} for all monsters");
            }
        }

        /// <summary>
        /// Provides helpful hint to the player
        /// </summary>
        private void ShowHelpfulHint(int remainingCookies)
        {
            string hint = remainingCookies > 0
                ? $"You have {remainingCookies} cookie(s) left over. This is the remainder!\nTime to submit your answer!"
                : "All cookies distributed! Time to submit your answer!";

            Debug.Log($"HINT: {hint}");

            // Can show this in a UI tooltip or help panel
        }

        private void OnDestroy()
        {
            _eventManager?.Unsubscribe<RemainderErrorEvent>(OnRemainderError);
        }
    }
}
