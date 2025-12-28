using UnityEngine;
using DG.Tweening;
using CookieGame.Core;

namespace CookieGame.Gameplay
{
    /// <summary>
    /// Cookie for sprite-based (non-UI) drag-and-drop
    /// Uses OnMouseDown/Drag/Up for 2D sprite interaction
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class Cookie_Sprite : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Collider2D _collider;

        [Header("Visual Feedback")]
        [SerializeField] private float _dragScale = 1.2f;
        [SerializeField] private float _dropDuration = 0.5f;
        [SerializeField] private int _draggingSortingOrder = 100;
        [SerializeField] private Color _validDropColor = Color.green;
        [SerializeField] private Color _invalidDropColor = Color.white;

        private Vector3 _originalPosition;
        private Vector3 _originalScale;
        private int _originalSortingOrder;
        private Color _originalColor;
        private bool _isDragging;
        private Camera _mainCamera;
        private EventManager _eventManager;
        private Vector3 _mouseOffset;
        private MonsterController _hoveredMonster;

        private void Awake()
        {
            if (_spriteRenderer == null)
                _spriteRenderer = GetComponent<SpriteRenderer>();

            if (_collider == null)
                _collider = GetComponent<Collider2D>();

            _mainCamera = Camera.main;
            _originalScale = transform.localScale;
        }

        private void Start()
        {
            _eventManager = ServiceLocator.Instance.Get<EventManager>();
            _originalPosition = transform.position;
            _originalSortingOrder = _spriteRenderer.sortingOrder;
            _originalColor = _spriteRenderer.color;
        }

        /// <summary>
        /// Called when mouse button is pressed on the cookie
        /// </summary>
        private void OnMouseDown()
        {
            if (!enabled || _isDragging) return;

            _isDragging = true;
            _originalPosition = transform.position;

            // Calculate offset between mouse and cookie center
            Vector3 mouseWorldPos = GetMouseWorldPosition();
            _mouseOffset = transform.position - mouseWorldPos;

            // Bring cookie to front
            _spriteRenderer.sortingOrder = _draggingSortingOrder;

            // Scale up for visual feedback
            transform.DOScale(_originalScale * _dragScale, 0.2f).SetEase(Ease.OutBack);

            Debug.Log("Cookie drag started (Sprite)");
        }

        /// <summary>
        /// Called continuously while dragging
        /// </summary>
        private void OnMouseDrag()
        {
            if (!_isDragging) return;

            // Move cookie to follow mouse
            Vector3 mouseWorldPos = GetMouseWorldPosition();
            transform.position = mouseWorldPos + _mouseOffset;

            // Check if hovering over a monster and update visual feedback
            MonsterController currentMonster = GetMonsterUnderCookie();
            UpdateHoverFeedback(currentMonster);
        }

        /// <summary>
        /// Called when mouse button is released
        /// </summary>
        private void OnMouseUp()
        {
            if (!_isDragging) return;

            _isDragging = false;

            // Reset color
            _spriteRenderer.color = _originalColor;

            // Check if dropped on a monster (use cookie position, not mouse)
            MonsterController targetMonster = GetMonsterUnderCookie();

            if (targetMonster != null)
            {
                // GDD V2.0: Valid drop - trigger FULL DISTRIBUTION ROUND to ALL monsters
                TriggerDistributionRound(targetMonster);
            }
            else
            {
                // Invalid drop - return to original position
                ReturnToOriginalPosition();
            }

            // Clear hover state
            _hoveredMonster = null;
        }

        /// <summary>
        /// Gets mouse position in world space
        /// </summary>
        private Vector3 GetMouseWorldPosition()
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = -_mainCamera.transform.position.z; // Distance from camera
            return _mainCamera.ScreenToWorldPoint(mousePos);
        }

        /// <summary>
        /// Checks if there's a monster under the current mouse position
        /// </summary>
        private MonsterController GetMonsterUnderMouse()
        {
            Vector3 mouseWorldPos = GetMouseWorldPosition();
            return GetMonsterAtPosition(mouseWorldPos);
        }

        /// <summary>
        /// Checks if there's a monster under the cookie's current position
        /// </summary>
        private MonsterController GetMonsterUnderCookie()
        {
            return GetMonsterAtPosition(transform.position);
        }

        /// <summary>
        /// Gets a monster at the specified world position using raycast
        /// </summary>
        private MonsterController GetMonsterAtPosition(Vector3 worldPosition)
        {
            // Disable cookie's own collider temporarily to avoid blocking raycast
            bool wasColliderEnabled = _collider.enabled;
            _collider.enabled = false;

            // Raycast from the position (check all hits)
            RaycastHit2D[] hits = Physics2D.RaycastAll(worldPosition, Vector2.zero);

            // Re-enable collider
            _collider.enabled = wasColliderEnabled;

            // Find monster in hits
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider != null)
                {
                    MonsterController monster = hit.collider.GetComponent<MonsterController>();
                    if (monster != null)
                    {
                        return monster;
                    }

                    // Check parent in case collider is on child object
                    monster = hit.collider.GetComponentInParent<MonsterController>();
                    if (monster != null)
                    {
                        return monster;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Updates visual feedback based on whether cookie is hovering over a valid drop target
        /// </summary>
        private void UpdateHoverFeedback(MonsterController currentMonster)
        {
            // Check if hover state changed
            if (currentMonster != _hoveredMonster)
            {
                _hoveredMonster = currentMonster;

                if (currentMonster != null)
                {
                    // Hovering over a monster - show valid drop feedback
                    _spriteRenderer.color = _validDropColor;
                    Debug.Log($"Hovering over {currentMonster.name}");
                }
                else
                {
                    // Not hovering over any monster - show invalid drop feedback
                    _spriteRenderer.color = _invalidDropColor;
                }
            }
        }

        /// <summary>
        /// Manual collection: ONE cookie dropped = ONE cookie to THIS monster only
        /// </summary>
        private void TriggerDistributionRound(MonsterController targetMonster)
        {
            // Add ONE cookie to THIS monster only (manual collection)
            targetMonster.AddOneCookie();
            
            _eventManager?.Publish(new CookieDroppedOnMonsterEvent
            {
                monsterId = targetMonster.GetMonsterId()
            });

            // Animate cookie to the monster's basket
            AnimateToBasket(targetMonster);

            Debug.Log($"Cookie dropped on {targetMonster.name} - Cookie added to basket");
        }

        /// <summary>
        /// Animates the cookie to the monster's basket and then returns to pool
        /// </summary>
        private void AnimateToBasket(MonsterController targetMonster)
        {
            // Get the basket position from the monster
            Transform basketTransform = targetMonster.GetBasketTransform();
            Vector3 basketPosition = basketTransform != null
                ? basketTransform.position
                : targetMonster.transform.position; // Fallback to monster position if no basket

            // Animate cookie to basket with arc motion (jump effect)
            transform.DOJump(basketPosition, 1f, 1, _dropDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    // Scale down and fade out
                    transform.DOScale(Vector3.zero, 0.2f);

                    // Return to pool after animation
                    DOVirtual.DelayedCall(0.2f, () =>
                    {
                        // Reset scale before returning to pool
                        transform.localScale = _originalScale;
                        ReturnToPool();
                    });
                });

            // Scale down during flight
            transform.DOScale(_originalScale * 0.5f, _dropDuration);

            // Reset sorting order
            _spriteRenderer.sortingOrder = _originalSortingOrder;

            Debug.Log($"Cookie animating to basket at {basketPosition}");
        }

        /// <summary>
        /// Returns cookie to original position with animation
        /// </summary>
        private void ReturnToOriginalPosition()
        {
            // Animate back to original position
            transform.DOMove(_originalPosition, _dropDuration)
                .SetEase(Ease.OutBack);

            transform.DOScale(_originalScale, 0.2f);
            _spriteRenderer.sortingOrder = _originalSortingOrder;

            Debug.Log("Cookie returned to spawn position");
        }

        /// <summary>
        /// Returns cookie to object pool
        /// </summary>
        private void ReturnToPool()
        {
            var poolManager = ServiceLocator.Instance.Get<CookieGame.Patterns.PoolManager>();
            poolManager?.Return("xCookie", this);
        }

        /// <summary>
        /// Resets cookie state when retrieved from pool
        /// </summary>
        public void ResetState()
        {
            transform.localScale = _originalScale;
            transform.position = _originalPosition; // Reset position
            _isDragging = false;
            _hoveredMonster = null;

            if (_spriteRenderer != null)
            {
                _spriteRenderer.sortingOrder = _originalSortingOrder;
                _spriteRenderer.color = _originalColor;
            }
        }

        /// <summary>
        /// Sets the spawn position for this cookie
        /// </summary>
        public void SetSpawnPosition(Vector3 position)
        {
            _originalPosition = position;
            transform.position = position;
        }
    }
}
