using UnityEngine;
using System.Collections.Generic;
using CookieGame.Core;
using CookieGame.Patterns;
using TMPro;

namespace CookieGame.Gameplay
{
    /// <summary>
    /// Spawns cookie's Spritesat designated positions
    /// Uses Object Pool pattern for performance
    /// GDD V2.0: Supports both sprite-based and UI-based cookies
    /// </summary>
    public class FakeCookieSpawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject _cookiePrefab; // Can be Cookie or Cookie_Sprite
        [SerializeField] private Transform _spawnParent;
        [SerializeField] private SpawnAreaBounds _spawnAreaBounds; // Defines spawn area (no collider)

        [Header("Spawn Settings")]
        [SerializeField] private int _cookiesPerRow = 5;
        [SerializeField] private Vector2 _cookieSpacing = new Vector2(1.2f, 1.2f);

        private PoolManager _poolManager;
        private EventManager _eventManager;
        private List<GameObject> _spawnedCookies = new List<GameObject>();

        // Cached sprite size (in world units at scale 1) for layout calculations
        private Vector2 _cookieBaseSize = Vector2.one;

        private const string cookiePoolID = "SpriteCookiePool";

        private void Start()
        {
            _eventManager = ServiceLocator.Instance.Get<EventManager>();

            // Setup object pooling
            _poolManager = ServiceLocator.Instance.Get<PoolManager>();
            if (_cookiePrefab != null && _poolManager != null)
            {
                // Get the Cookie or Cookie_Sprite component from prefab
                var cookieSR = _cookiePrefab.GetComponent<SpriteRenderer>();

                if (cookieSR != null)
                {
                    _poolManager.CreatePool(cookiePoolID, cookieSR, 20);
                }
                else
                {
                    Debug.LogError("CookieSpawner: Cookie prefab must have Cookie_Sprite component!");
                }

                // Cache cookie sprite size for layout / non-overlap calculations
                if (cookieSR)
                {
                        // Use sprite bounds (local units) which correspond to world units at scale (1,1,1)
                        _cookieBaseSize = cookieSR.sprite.bounds.size;
                }

                // Validate spawn area bounds
                if (_spawnAreaBounds == null)
                {
                    Debug.LogError(
                        "CookieSpawner: SpawnAreaBounds not assigned! Add SpawnAreaBounds component and assign it.");
                }

                // Subscribe to question generated event
                _eventManager?.Subscribe<QuestionGeneratedEvent>(OnQuestionGenerated);
                _eventManager?.Subscribe<CookieDroppedOnMonsterEvent>(OnCookieGrabbed);
            }
        }

        private void OnQuestionGenerated(QuestionGeneratedEvent eventData)
        {
            // Clear existing cookies
            //ClearCookies();
            _poolManager.ReturnAll<SpriteRenderer>(cookiePoolID);
            _spawnedCookies.Clear();
            SpawnCookieSprites(eventData.dividend);
        }

        private void OnCookieGrabbed(CookieDroppedOnMonsterEvent eventData)
        {
            if (_spawnedCookies == null || _spawnedCookies.Count == 0) return;

            var c = _spawnedCookies[^1].GetComponent<SpriteRenderer>();

            if (c != null)
            {
                _poolManager.Return(cookiePoolID, c);
                _spawnedCookies.Remove(c.gameObject);
            }
        }
        
        /// <summary>
        /// Spawns the specified number of cookies in a grid pattern
        /// </summary>
        public void SpawnCookieSprites(int count)
        {
            if (_cookiePrefab == null)
            {
                Debug.LogError("CookieSpawner: Cookie prefab not assigned!");
                return;
            }

            if (_spawnAreaBounds == null)
            {
                Debug.LogError("CookieSpawner: SpawnAreaBounds not assigned!");
                return;
            }

            // Calculate grid positions within spawn area
            List<Vector3> positions = CalculateGridPositions(count, out Vector3 cookieScale);

            // Spawn cookies
            for (int i = 0; i < count && i < positions.Count; i++)
            {
                SpawnCookieAt(positions[i], i, cookieScale);
            }

            Debug.Log($"CookieSpawner: Spawned {_spawnedCookies.Count}/{count} cookies");
        }

        /// <summary>
        /// Calculates grid positions within the spawn area
        /// </summary>
        private List<Vector3> CalculateGridPositions(int count, out Vector3 cookieScale)
        {
            List<Vector3> positions = new List<Vector3>();

            // Default scale (no resizing) in case we cannot compute sprite bounds
            cookieScale = Vector3.one;

            // Get spawn area bounds from SpawnAreaBounds
            Bounds bounds = GetSpawnBounds();

            // Calculate grid dimensions (columns first so we can derive rows)
            int columns = Mathf.Min(count, Mathf.Max(1, _cookiesPerRow));
            int rows = Mathf.CeilToInt((float)count / columns);



            // Compute a uniform scale so that:
            //  - Cookies do NOT overlap (using sprite bounds as their size)
            //  - All cookies (including requested spacing) fit inside the spawn bounds
            float areaWidth = bounds.size.x;
            float areaHeight = bounds.size.y;

            float spacingX = Mathf.Max(0f, _cookieSpacing.x);
            float spacingY = Mathf.Max(0f, _cookieSpacing.y);

            // Width/height available for the actual cookie sprites (excluding spacing)
            float availableWidthForCookies = areaWidth - (columns - 1) * spacingX;
            float availableHeightForCookies = areaHeight - (rows - 1) * spacingY;

            // Guard against degenerate areas
            /*if (availableWidthForCookies <= 0f || availableHeightForCookies <= 0f)
            {
                // Fallback: keep default scale and center-based positions
                Vector3 areaCenter = bounds.center;
                float totalWidthFallback = (columns - 1) * spacingX;
                float totalHeightFallback = (rows - 1) * spacingY;

                Vector3 startPosFallback = new Vector3(
                    areaCenter.x - totalWidthFallback * 0.5f,
                    areaCenter.y + totalHeightFallback * 0.5f,
                    0f
                );

                for (int i = 0; i < count; i++)
                {
                    int rowIndex = i / columns;
                    int colIndex = i % columns;

                    Vector3 position = new Vector3(
                        startPosFallback.x + colIndex * spacingX,
                        startPosFallback.y - rowIndex * spacingY,
                        0f
                    );

                    positions.Add(position);
                }

                return positions;
            }
            */

            // Maximum scale factors along each axis so that the entire grid fits
            float maxScaleX = availableWidthForCookies / (columns * _cookieBaseSize.x);
            float maxScaleY = availableHeightForCookies / (rows * _cookieBaseSize.y);

            // Use the smaller of the two to keep aspect ratio and ensure we stay inside the bounds
            float uniformScale = Mathf.Min(maxScaleX, maxScaleY, 1f);

            // Safety check in case something went wrong with the math
            if (uniformScale <= 0f || float.IsNaN(uniformScale) || float.IsInfinity(uniformScale))
            {
                uniformScale = 1f;
            }

            cookieScale = new Vector3(uniformScale, uniformScale, 1f);

            // Actual cookie world size after scaling
            float cookieWidth = _cookieBaseSize.x * uniformScale;
            float cookieHeight = _cookieBaseSize.y * uniformScale;

            // Compute the top-left cookie center so that the entire grid sits inside the bounds
            float left = bounds.min.x + cookieWidth * 0.5f;
            float top = bounds.max.y - cookieHeight * 0.5f;

            for (int i = 0; i < count; i++)
            {
                int rowIndex = i / columns;
                int colIndex = i % columns;

                float x = left + colIndex * (cookieWidth + spacingX);
                float y = top - rowIndex * (cookieHeight + spacingY);

                Vector3 position = new Vector3(x, y, 0f);

                positions.Add(position);
            }

            return positions;
        }

        /// <summary>
        /// Spawns a single cookie at the given position
        /// </summary>
        private void SpawnCookieAt(Vector3 position, int index, Vector3 cookieScale)
        {
            GameObject cookieObj = null;

            // Use object pooling if enabled
            if (_poolManager != null)
            {
                // Try to get Cookie_Sprite from pool first
                var cookie_Sprite = _poolManager.Get<SpriteRenderer>(cookiePoolID);
                if (cookie_Sprite != null)
                {
                    cookieObj = cookie_Sprite.gameObject;
                }
            }
            else
            {
                // Instantiate new cookie
                cookieObj = Instantiate(_cookiePrefab, position, Quaternion.identity, _spawnParent);
            }

            if (cookieObj == null) return;

            cookieObj.name = $"Cookie_{index}";
            cookieObj.transform.position = position;

            // Apply calculated scale so cookies do not overlap and remain inside the spawn bounds
            if (cookieScale != Vector3.zero &&
                !float.IsNaN(cookieScale.x) && !float.IsInfinity(cookieScale.x))
            {
                cookieObj.transform.localScale = cookieScale;
            }

            // Update TextMeshPro text on the fake cookie (3D TextMeshPro component)
            UpdateCookieText(cookieObj, index + 1);

            _spawnedCookies.Add(cookieObj);
        }

        /// <summary>
        /// Updates the TextMeshPro text on a fake cookie
        /// </summary>
        private void UpdateCookieText(GameObject cookieObj, int cookieNumber)
        {
            // Find TextMeshPro component in children (3D TextMeshPro)
            TextMeshPro textMeshPro = cookieObj.GetComponentInChildren<TextMeshPro>();
            
            if (textMeshPro != null)
            {
                textMeshPro.text = cookieNumber.ToString();
            }
            else
            {
                Debug.LogWarning($"FakeCookieSpawner: TextMeshPro component not found on cookie {cookieObj.name}");
            }
        }

        /// <summary>
        /// Clears all spawned cookies
        /// </summary>
        private void ClearCookies()
        {
            if (_poolManager != null)
            {
                // Return to pool - try both component types
                foreach (GameObject cookieObj in _spawnedCookies)
                {
                    if (cookieObj == null) continue;

                    Cookie_Sprite cookieSprite = cookieObj.GetComponent<Cookie_Sprite>();
                    if (cookieSprite != null)
                    {
                        _poolManager.Return(cookiePoolID, cookieSprite);
                    }
                }
            }
            else
            {
                // Destroy instantiated cookies
                foreach (GameObject cookie in _spawnedCookies)
                {
                    if (cookie != null)
                    {
                        Destroy(cookie);
                    }
                }
            }

            _spawnedCookies.Clear();
        }

        /// <summary>
        /// Gets spawn area bounds from SpawnAreaBounds component
        /// </summary>
        private Bounds GetSpawnBounds()
        {
            if (_spawnAreaBounds != null)
            {
                return _spawnAreaBounds.GetBounds();
            }

            // Default bounds if not assigned
            Debug.LogWarning("CookieSpawner: No spawn area defined! Using default bounds.");
            return new Bounds(transform.position, new Vector3(7f, 4f, 0f));
        }

        private void OnDestroy()
        {
            _eventManager?.Unsubscribe<QuestionGeneratedEvent>(OnQuestionGenerated);
            ClearCookies();
        }

        // OnDrawGizmos removed - SpawnAreaBounds component draws its own gizmo
    }
}
