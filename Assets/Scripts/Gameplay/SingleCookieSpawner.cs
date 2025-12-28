using UnityEngine;
using System.Collections.Generic;
using CookieGame.Core;
using CookieGame.Patterns;

namespace CookieGame.Gameplay
{
    /// <summary>
    /// Spawns cookies one at a time at the spawner location.
    /// Starts with one cookie. When a cookie is grabbed by a monster,
    /// spawns a new cookie at the same location until the dividend limit is reached.
    /// </summary>
    public class SingleCookieSpawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject _cookiePrefab; // Can be Cookie or Cookie_Sprite
        [SerializeField] private Transform _spawnParent;

        [Header("Settings")]
        [SerializeField] private bool _useObjectPooling = true;

        [Header("Visual Settings")]
        [SerializeField] private int _baseSortingOrder = 1;

        private PoolManager _poolManager;
        private EventManager _eventManager;
        private List<GameObject> _spawnedCookies = new List<GameObject>();

        // Cookie spawning tracking
        private int _maxCookies = 0; // Maximum cookies to spawn (from dividend)
        private int _cookiesSpawned = 0; // How many cookies have been spawned so far

        private void Start()
        {
            _eventManager = ServiceLocator.Instance.Get<EventManager>();

            // Setup object pooling if enabled
            if (_useObjectPooling)
            {
                _poolManager = ServiceLocator.Instance.Get<PoolManager>();
                if (_cookiePrefab != null && _poolManager != null)
                {
                    // Get the Cookie or Cookie_Sprite component from prefab
                    Cookie_Sprite cookieSprite = _cookiePrefab.GetComponent<Cookie_Sprite>();

                    if (cookieSprite != null)
                    {
                        _poolManager.CreatePool("xCookie", cookieSprite, 20);
                    }
                    else
                    {
                        Debug.LogError("SingleCookieSpawner: Cookie prefab must have Cookie or Cookie_Sprite component!");
                        _useObjectPooling = false;
                    }
                }
            }

            // Subscribe to events
            _eventManager?.Subscribe<QuestionGeneratedEvent>(OnQuestionGenerated);
            _eventManager?.Subscribe<CookieDroppedOnMonsterEvent>(OnCookieGrabbed);
        }

        /// <summary>
        /// Called when a new question is generated
        /// </summary>
        private void OnQuestionGenerated(QuestionGeneratedEvent eventData)
        {
            // Clear existing cookies
            ClearCookies();

            // Initialize cookie spawning tracking
            _maxCookies = eventData.dividend;
            _cookiesSpawned = 0;

            // Spawn the first cookie at the spawner location
            SpawnSingleCookie();

            Debug.Log($"SingleCookieSpawner: New question - dividend: {_maxCookies}, spawned first cookie");
        }

        /// <summary>
        /// Called when a cookie is grabbed by a monster
        /// Spawns a new cookie if we haven't reached the max count
        /// </summary>
        private void OnCookieGrabbed(CookieDroppedOnMonsterEvent eventData)
        {
            // Check if we can spawn more cookies
            if (_cookiesSpawned < _maxCookies)
            {
                SpawnSingleCookie();
            }
            else
            {
                Debug.Log($"SingleCookieSpawner: Maximum cookies ({_maxCookies}) reached. No more cookies will spawn.");
            }

            Debug.Log($"SingleCookieSpawner: Cookie Grabbed!");
        }

        /// <summary>
        /// Spawns a single cookie at the spawner's transform position
        /// </summary>
        private void SpawnSingleCookie()
        {
            if (_cookiePrefab == null)
            {
                Debug.LogError("SingleCookieSpawner: Cookie prefab not assigned!");
                return;
            }

            // Use spawner's transform position as the spawn location
            Vector3 spawnPosition = _spawnParent.position;

            // Spawn the cookie at this position
            GameObject cookieObj = null;

            // Use object pooling if enabled
            if (_useObjectPooling && _poolManager != null)
            {
                // Try to get Cookie_Sprite from pool first
                Cookie_Sprite cookie_Sprite = _poolManager.Get<Cookie_Sprite>("xCookie");
                if (cookie_Sprite != null)
                {
                    cookieObj = cookie_Sprite.gameObject;
                }
            }
            else
            {
                // Instantiate new cookie
                cookieObj = Instantiate(_cookiePrefab, spawnPosition, Quaternion.identity, _spawnParent);
            }

            if (cookieObj == null)
            {
                Debug.LogError("SingleCookieSpawner: Failed to create cookie object!");
                return;
            }

            cookieObj.name = $"Cookie_{_cookiesSpawned}";
            cookieObj.transform.position = spawnPosition;

            // Reset scale to default
            cookieObj.transform.localScale = Vector3.one;

            // Setup sprite-based cookie
            Cookie_Sprite cookieSprite = cookieObj.GetComponent<Cookie_Sprite>();
            if (cookieSprite != null)
            {
                cookieSprite.SetSpawnPosition(spawnPosition);
            }

            // Set sorting order for sprite renderer
            SpriteRenderer spriteRenderer = cookieObj.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = _baseSortingOrder;
            }

            // Ensure collider is enabled for clicking
            Collider2D collider = cookieObj.GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = true;
            }

            _spawnedCookies.Add(cookieObj);
            _cookiesSpawned++;

            Debug.Log($"SingleCookieSpawner: Spawned cookie {_cookiesSpawned}/{_maxCookies} at position {spawnPosition}");
        }

        /// <summary>
        /// Clears all spawned cookies
        /// </summary>
        private void ClearCookies()
        {
            if (_useObjectPooling && _poolManager != null)
            {
                // Return to pool - try both component types
                foreach (GameObject cookieObj in _spawnedCookies)
                {
                    if (cookieObj == null) continue;

                    Cookie_Sprite cookieSprite = cookieObj.GetComponent<Cookie_Sprite>();
                    if (cookieSprite != null)
                    {
                        _poolManager.Return("xCookie", cookieSprite);
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

        private void OnDestroy()
        {
            _eventManager?.Unsubscribe<QuestionGeneratedEvent>(OnQuestionGenerated);
            _eventManager?.Unsubscribe<CookieDroppedOnMonsterEvent>(OnCookieGrabbed);
            ClearCookies();
        }
    }
}
