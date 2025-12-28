using UnityEngine;

namespace CookieGame.Gameplay
{
    /// <summary>
    /// Alternative to BoxCollider2D for defining spawn area
    /// Uses Transform scale instead of collider (won't block clicks)
    /// </summary>
    public class SpawnAreaBounds : MonoBehaviour
    {
        [Header("Spawn Area Size")]
        [SerializeField] private Vector2 _size = new Vector2(7f, 4f);

        /// <summary>
        /// Gets the bounds of this spawn area
        /// </summary>
        public Bounds GetBounds()
        {
            return new Bounds(transform.position, new Vector3(_size.x, _size.y, 0f));
        }

        /// <summary>
        /// Visualize spawn area in editor (same as BoxCollider2D gizmo)
        /// </summary>
        private void OnDrawGizmos()
        {
            Bounds bounds = GetBounds();

            // Draw filled box
            Gizmos.color = new Color(1f, 0.8f, 0f, 0f); // transparent white
            Gizmos.DrawCube(bounds.center, bounds.size);

            // Draw wireframe
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }
    }
}
