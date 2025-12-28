using UnityEngine;
using DG.Tweening;

namespace CookieGame.VFX
{
    /// <summary>
    /// Utility class for common animations
    /// Provides reusable animation methods using DOTween
    /// </summary>
    public static class AnimationController
    {
        /// <summary>
        /// Bounces a transform
        /// </summary>
        public static Tween Bounce(Transform target, float height = 0.5f, float duration = 0.5f)
        {
            return target.DOJump(target.position, height, 1, duration)
                .SetEase(Ease.OutQuad);
        }

        /// <summary>
        /// Shakes a transform
        /// </summary>
        public static Tween Shake(Transform target, float strength = 0.3f, float duration = 0.5f)
        {
            return target.DOShakePosition(duration, strength, 10)
                .SetEase(Ease.InOutQuad);
        }

        /// <summary>
        /// Pulses a transform (scale up and down)
        /// </summary>
        public static Sequence Pulse(Transform target, float scaleMultiplier = 1.2f, float duration = 0.3f)
        {
            Vector3 originalScale = target.localScale;
            Sequence sequence = DOTween.Sequence();

            sequence.Append(target.DOScale(originalScale * scaleMultiplier, duration / 2f).SetEase(Ease.OutQuad));
            sequence.Append(target.DOScale(originalScale, duration / 2f).SetEase(Ease.InQuad));

            return sequence;
        }

        /// <summary>
        /// Fades in a CanvasGroup
        /// </summary>
        public static Tween FadeIn(CanvasGroup target, float duration = 0.5f)
        {
            target.alpha = 0f;
            return target.DOFade(1f, duration).SetEase(Ease.OutQuad);
        }

        /// <summary>
        /// Fades out a CanvasGroup
        /// </summary>
        public static Tween FadeOut(CanvasGroup target, float duration = 0.5f)
        {
            return target.DOFade(0f, duration).SetEase(Ease.InQuad);
        }

        /// <summary>
        /// Rotates a transform continuously
        /// </summary>
        public static Tween RotateContinuous(Transform target, float duration = 2f)
        {
            return target.DORotate(new Vector3(0f, 0f, 360f), duration, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart);
        }

        /// <summary>
        /// Pop-in animation (scale from zero)
        /// </summary>
        public static Tween PopIn(Transform target, float duration = 0.5f)
        {
            target.localScale = Vector3.zero;
            return target.DOScale(Vector3.one, duration)
                .SetEase(Ease.OutBack);
        }

        /// <summary>
        /// Pop-out animation (scale to zero)
        /// </summary>
        public static Tween PopOut(Transform target, float duration = 0.3f)
        {
            return target.DOScale(Vector3.zero, duration)
                .SetEase(Ease.InBack);
        }

        /// <summary>
        /// Slide in from direction
        /// </summary>
        public static Tween SlideIn(RectTransform target, Vector2 direction, float distance = 1000f, float duration = 0.5f)
        {
            Vector2 originalPosition = target.anchoredPosition;
            target.anchoredPosition = originalPosition + (direction * distance);

            return target.DOAnchorPos(originalPosition, duration)
                .SetEase(Ease.OutCubic);
        }

        /// <summary>
        /// Slide out to direction
        /// </summary>
        public static Tween SlideOut(RectTransform target, Vector2 direction, float distance = 1000f, float duration = 0.5f)
        {
            Vector2 targetPosition = target.anchoredPosition + (direction * distance);

            return target.DOAnchorPos(targetPosition, duration)
                .SetEase(Ease.InCubic);
        }

        /// <summary>
        /// Number counter animation
        /// </summary>
        public static Tween CountTo(int from, int to, float duration, System.Action<int> onUpdate)
        {
            // Using DOTween's virtual tweener with proper callback
            return DOVirtual.Float(from, to, duration, (value) => {
                onUpdate?.Invoke(Mathf.RoundToInt(value));
            }).SetEase(Ease.OutQuad);
        }

        /// <summary>
        /// Color flash animation
        /// </summary>
        public static Sequence ColorFlash(SpriteRenderer target, Color flashColor, float duration = 0.2f)
        {
            Color originalColor = target.color;
            Sequence sequence = DOTween.Sequence();

            sequence.Append(target.DOColor(flashColor, duration / 2f));
            sequence.Append(target.DOColor(originalColor, duration / 2f));

            return sequence;
        }
    }
}
