using UnityEngine;
using UnityEngine.UI;

namespace CookieGame.UI
{
    /// <summary>
    /// Splash screen - shows game logo
    /// </summary>
    public class SplashScreen : UIScreen
    {
        [Header("Logo")]
        [SerializeField] private Image _logoImage;

        protected override void OnShow()
        {
            // Add any logo animation here
            if (_logoImage != null)
            {
                // Pulsing animation for logo (requires DOTween package)
                // Install DOTween and uncomment the line below:
                // _logoImage.transform.DOScale(Vector3.one, 1f).SetEase(Ease.OutElastic).SetLoops(-1, LoopType.Yoyo);

                // Simple fallback without DOTween
                _logoImage.transform.localScale = Vector3.one;
            }
        }

        protected override void OnHide()
        {
            // Cancel any ongoing animations
            if (_logoImage != null)
            {
                // DOTween.Kill(_logoImage.transform);
            }
        }
    }
}
