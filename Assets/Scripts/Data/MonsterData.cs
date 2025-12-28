using UnityEngine;

namespace CookieGame.Data
{
    /// <summary>
    /// Data class for monster configuration
    /// Each monster type has unique visual and audio properties
    /// </summary>
    [CreateAssetMenu(fileName = "MonsterData", menuName = "Cookie Game/Monster Data")]
    public class MonsterData : ScriptableObject
    {
        [Header("Visual Settings")]
        public string monsterName;
        public Color monsterColor = Color.white;
        public Sprite idleSprite;
        public Sprite happySprite;
        public Sprite sadSprite;
        public Sprite eatingSprite;

        [Header("Animation")]
        public AnimationClip idleAnimation;
        public AnimationClip happyAnimation;
        public AnimationClip sadAnimation;
        public AnimationClip eatingAnimation;

        [Header("Audio")]
        public AudioClip eatSound;
        public AudioClip happySound;
        public AudioClip sadSound;

        [Header("Particle Effects")]
        public GameObject happyParticles;
        public GameObject sadParticles;

        [Header("Behavior")]
        [Tooltip("How fast the monster reacts to input")]
        [Range(0.5f, 2f)]
        public float reactionSpeed = 1f;
    }
}
