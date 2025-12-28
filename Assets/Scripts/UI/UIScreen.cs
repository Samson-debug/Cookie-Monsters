using UnityEngine;
#if DOTWEEN_ENABLED
using DG.Tweening;
#endif

namespace CookieGame.UI
{
    /// <summary>
    /// Base class for all UI screens
    /// Follows Open/Closed Principle - screens can extend this base
    /// Provides common show/hide functionality with animations
    /// </summary>
    public abstract class UIScreen : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] protected float _animationDuration = 0.3f;
        [SerializeField] protected bool _useAnimations = false; // Set to true when DOTween is available

        protected CanvasGroup _canvasGroup;
        protected RectTransform _rectTransform;
        protected bool _isVisible;

        public virtual void Initialize()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            _rectTransform = GetComponent<RectTransform>();

            // Start hidden by default (invisible but GameObject stays active)
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }

            _isVisible = false;
        }

        /// <summary>
        /// Shows the screen with animation
        /// Uses CanvasGroup to control visibility - GameObject stays active
        /// </summary>
        public virtual void Show()
        {
            // Keep GameObject active - only use CanvasGroup for visibility
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            _isVisible = true;

            if (_canvasGroup != null)
            {
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;

                // Instant show without animation
                _canvasGroup.alpha = 1f;
                if (_rectTransform != null)
                {
                    _rectTransform.localScale = Vector3.one;
                }
            }

            OnShow();
        }

        /// <summary>
        /// Hides the screen with animation
        /// Uses CanvasGroup to control visibility - GameObject stays active
        /// </summary>
        public virtual void Hide()
        {
            _isVisible = false;

            if (_canvasGroup != null)
            {
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;

                // Instant hide without animation
                _canvasGroup.alpha = 0f;
                // Don't disable GameObject - keep it active for event subscriptions
                OnHide();
            }
            else
            {
                // Fallback: if no CanvasGroup, disable GameObject
                gameObject.SetActive(false);
                OnHide();
            }
        }

        /// <summary>
        /// Called when screen is shown
        /// Override in derived classes for custom behavior
        /// </summary>
        protected virtual void OnShow() { }

        /// <summary>
        /// Called when screen is hidden
        /// Override in derived classes for custom behavior
        /// </summary>
        protected virtual void OnHide() { }

        public bool IsVisible() => _isVisible;
    }
}
