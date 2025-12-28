using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using CookieGame.Core;
using CookieGame.Data;
using CookieGame.Audio;
using TMPro;
using Spine.Unity;

namespace CookieGame.Gameplay
{
    /// <summary>
    /// Monster controller with animations and cookie reception
    /// Follows Single Responsibility Principle
    /// Handles visual feedback and state management for monsters
    /// </summary>
    public class MonsterController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SkeletonAnimation _skeletonAnimation;
        [SerializeField] private Transform _basketTransform;
        [SerializeField] private TextMeshProUGUI _cookieCountText;
        [SerializeField] private int _monsterIndex;

        private EventManager _eventManager;
        private AudioManager _audioManager;
        private int _cookieCount;
        private int _roundCount; // GDD V2.0: Tracks rounds for this monster
        private Vector3 _originalScale;

        private string idle;
        private string happy;
        private string sad;

        private void Awake()
        {
            _originalScale = transform.localScale;

            if (_skeletonAnimation == null)
                _skeletonAnimation = GetComponent<SkeletonAnimation>();
        }

        private void Start()
        {
            _eventManager = ServiceLocator.Instance.Get<EventManager>();
            _audioManager = ServiceLocator.Instance.Get<AudioManager>();

            _cookieCount = 0;
            _roundCount = 0;
            UpdateCookieCountDisplay();

            // GDD V2.0: Subscribe to new distribution events
            _eventManager?.Subscribe<QuestionGeneratedEvent>(OnQuestionGenerated);
            _eventManager?.Subscribe<AllMonstersReceivedCookieEvent>(OnDistributionRound);
            
            SetAnimationNames();
        }

        private void SetAnimationNames()
        {
            if (_skeletonAnimation == null) return;

            var skeletonData = _skeletonAnimation.skeleton.Data;

            foreach (var anim in skeletonData.Animations)
            {
                if (string.Equals(anim.Name, "happy", StringComparison.OrdinalIgnoreCase))
                {
                    happy = anim.Name;
                }
                else if(string.Equals(anim.Name, "idle", StringComparison.OrdinalIgnoreCase))
                {
                    idle = anim.Name;
                }
                else if (string.Equals(anim.Name, "sad", StringComparison.OrdinalIgnoreCase))
                {
                    sad = anim.Name;
                }
            }
            
            Debug.Log($"[{gameObject.name} Animations: {happy}, {idle}, {sad}");
        }

        private void OnQuestionGenerated(QuestionGeneratedEvent eventData)
        {
            ResetCookies();
            // Play idle animation when new question starts
            PlayIdleAnimation();
        }

        /// <summary>
        /// GDD V2.0: Called when a distribution round happens (all monsters receive cookies)
        /// </summary>
        private void OnDistributionRound(AllMonstersReceivedCookieEvent eventData)
        {
            // Increment round counter
            _roundCount++;
            _cookieCount++;

            // Update displays
            UpdateCookieCountDisplay();

            // Play eating animation with slight delay based on monster index for sequential effect
            float delay = _monsterIndex * 0.15f;
            DOVirtual.DelayedCall(delay, () =>
            {
                PlayEatingAnimation();

                // Play eating sound
            });

            Debug.Log($"Monster {_monsterIndex}: Round {_roundCount}, Cookies: {_cookieCount}");
        }

        /// <summary>
        /// Manual cookie collection - called when player drags ONE cookie to this monster
        /// </summary>
        public void AddOneCookie()
        {
            _cookieCount++;
            UpdateCookieCountDisplay();
            PlayEatingAnimation();

            // Play eating sound

            Debug.Log($"Monster {_monsterIndex}: Cookie added. Total: {_cookieCount}");
        }

        /// <summary>
        /// Called when a cookie is dropped on this monster
        /// Distributes cookies to all monsters automatically
        /// </summary>
        public void ReceiveCookie()
        {
            // Play eating animation
            PlayEatingAnimation();

            // Add cookie to this monster
            _cookieCount++;
            UpdateCookieCountDisplay();

            // Publish event so other monsters also receive cookies
            _eventManager?.Publish(new CookieDistributedEvent
            {
                monsterIndex = _monsterIndex
            });

            // Distribute to all other monsters
            DistributeToAllMonsters();

            // Play eating sound
        }

        private void DistributeToAllMonsters()
        {
            // Find all monsters and give them a cookie
            MonsterController[] allMonsters = FindObjectsOfType<MonsterController>();

            foreach (var monster in allMonsters)
            {
                if (monster != this && monster._cookieCount == _cookieCount - 1)
                {
                    monster.ReceiveCookieFromDistribution();
                }
            }
        }

        /// <summary>
        /// Called when receiving a cookie from automatic distribution
        /// </summary>
        public void ReceiveCookieFromDistribution()
        {
            _cookieCount++;
            UpdateCookieCountDisplay();
            PlayEatingAnimation();
        }

        /// <summary>
        /// Plays the eating animation
        /// </summary>
        private void PlayEatingAnimation()
        {
            // Scale animation for eating
            transform.DOScale(_originalScale * 1.1f, 0.2f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    transform.DOScale(_originalScale, 0.2f);
                });
        }

        /// <summary>
        /// Plays the idle animation on loop
        /// </summary>
        private void PlayIdleAnimation()
        {
            Debug.Log($"[MonsterController] [{gameObject.name}] happy aniamtion playing!");
            if (_skeletonAnimation != null)
            {
                _skeletonAnimation.AnimationState.SetAnimation(0, idle, true);
                Debug.Log($"[MonsterController] [{gameObject.name}] happy aniamtion played!");
            }
        }

        /// <summary>
        /// Plays the happy animation on loop
        /// </summary>
        private void PlayHappyAnimation()
        {
            Debug.Log($"[MonsterController] [{gameObject.name}] happy aniamtion playing!");
            if (_skeletonAnimation != null)
            {
                _skeletonAnimation.AnimationState.SetAnimation(0, happy, true);
                Debug.Log($"[MonsterController] [{gameObject.name}] happy aniamtion played!");
            }
        }

        /// <summary>
        /// Plays the sad animation on loop
        /// </summary>
        private void PlaySadAnimation()
        {
            Debug.Log($"[MonsterController] [{gameObject.name}] sad aniamtion playing!");
            if (_skeletonAnimation != null)
            {
                _skeletonAnimation.AnimationState.SetAnimation(0, sad, true);
                Debug.Log($"[MonsterController] [{gameObject.name}] sad aniamtion played!");
            }
        }

        /// <summary>
        /// Shows happy reaction for correct answer
        /// </summary>
        public void ShowHappyReaction()
        {
            // Play happy animation on loop
            PlayHappyAnimation();

            // Jump animation
            transform.DOJump(transform.position, 0.5f, 1, 0.5f);

            // Play happy sound
            // Show particles
        }

        /// <summary>
        /// Shows sad reaction for wrong answer
        /// </summary>
        public void ShowSadReaction()
        {
            // Play sad animation on loop
            PlaySadAnimation();

            // Shake animation
            transform.DOShakePosition(0.5f, 0.2f, 10);

            // Play sad sound

            // Show particles
        }

        /// <summary>
        /// Updates the cookie count display
        /// </summary>
        private void UpdateCookieCountDisplay()
        {
            if (_cookieCountText != null)
            {
                _cookieCountText.text = _cookieCount.ToString();
            }
        }

        /// <summary>
        /// Resets cookie count for new question
        /// </summary>
        public void ResetCookies()
        {
            _cookieCount = 0;
            _roundCount = 0;
            UpdateCookieCountDisplay();
        }

        public int GetCookieCount() => _cookieCount;
        public void SetMonsterIndex(int index) => _monsterIndex = index;
        public int GetMonsterId() => _monsterIndex; // GDD V2.0: ID for event tracking
        public Transform GetBasketTransform() => _basketTransform; // Get basket position for cookie animations

        private void OnDestroy()
        {
            _eventManager?.Unsubscribe<QuestionGeneratedEvent>(OnQuestionGenerated);
            _eventManager?.Unsubscribe<AllMonstersReceivedCookieEvent>(OnDistributionRound);
        }
    }
}
