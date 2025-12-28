using System;
using System.Collections.Generic;
using UnityEngine;

namespace CookieGame.Core
{
    /// <summary>
    /// Event system using Observer pattern
    /// Allows decoupled communication between systems
    /// </summary>
    public class EventManager
    {
        private readonly Dictionary<Type, Delegate> _eventDictionary = new Dictionary<Type, Delegate>();

        public void Subscribe<T>(Action<T> listener) where T : struct
        {
            var eventType = typeof(T);
            if (_eventDictionary.TryGetValue(eventType, out var existingDelegate))
            {
                _eventDictionary[eventType] = Delegate.Combine(existingDelegate, listener);
            }
            else
            {
                _eventDictionary[eventType] = listener;
            }
        }

        public void Unsubscribe<T>(Action<T> listener) where T : struct
        {
            var eventType = typeof(T);
            if (_eventDictionary.TryGetValue(eventType, out var existingDelegate))
            {
                var newDelegate = Delegate.Remove(existingDelegate, listener);
                if (newDelegate == null)
                {
                    _eventDictionary.Remove(eventType);
                }
                else
                {
                    _eventDictionary[eventType] = newDelegate;
                }
            }
        }

        public void Publish<T>(T eventData) where T : struct
        {
            var eventType = typeof(T);
            if (_eventDictionary.TryGetValue(eventType, out var existingDelegate))
            {
                (existingDelegate as Action<T>)?.Invoke(eventData);
            }
        }

        public void Clear()
        {
            _eventDictionary.Clear();
        }
    }

    // ========== Game State Events ==========
    public struct GameStartedEvent { }
    public struct GamePausedEvent { }
    public struct GameResumedEvent { }
    public struct GameOverEvent { public int finalScore; public float accuracy; }

    // ========== Question Events ==========
    public struct QuestionGeneratedEvent
    {
        public int dividend;
        public int divisor;
        public int correctAnswer;
        public int remainder; // NEW: for remainder support
    }

    // ========== Distribution Events (NEW for GDD V2.0) ==========
    public struct DistributionRoundStartedEvent
    {
        public int roundNumber;
        public int remainingCookies;
    }

    public struct DistributionRoundCompletedEvent
    {
        public int roundNumber;
        public int cookiesPerMonster;
        public int remainingCookies;
        public bool isCorrect;
    }

    public struct CookieDistributedEvent
    {
        public int monsterIndex;
        public int roundNumber; // NEW: track which round
    }

    public struct AllMonstersReceivedCookieEvent
    {
        public int roundNumber;
    }

    public struct CookieDroppedOnMonsterEvent // NEW: when player drops cookie on a monster
    {
        public int monsterId;
    }

    // ========== Answer Events ==========
    public struct AnswerSubmittedEvent
    {
        public bool isCorrect;
        public int submittedAnswer;
        public int correctAnswer; // NEW: for showing correct answer
        public float timeTaken; // NEW: time taken to answer
    }

    public struct RemainderErrorEvent // NEW: when player tries to distribute with remainder
    {
        public int remainingCookies;
        public int divisor;
    }

    // ========== Score Events ==========
    public struct ScoreUpdatedEvent
    {
        public int newScore;
        public int totalQuestions;
        public int correctAnswers;
        public float multiplier; // NEW: current multiplier
    }

    public struct RoundScoreAddedEvent // NEW: score for completing a distribution round
    {
        public int roundScore;
        public int totalScore;
    }

    // ========== Lives Events ==========
    public struct LivesUpdatedEvent { public int remainingLives; }

    // ========== Timer Events ==========
    public struct TimerUpdatedEvent { public float remainingTime; }

    // ========== UI Events ==========
    public struct ScreenChangedEvent { public string screenName; }

    // ========== Cookie Pile Events (NEW for GDD V2.0) ==========
    public struct CookiePileUpdatedEvent
    {
        public int remainingCookies;
        public int totalCookies;
    }

    // ========== Answer Input Events (NEW for GDD V2.0) ==========
    public struct ShowAnswerInputEvent { }
    public struct HideAnswerInputEvent { }

    // ========== Audio Events ==========
    public struct SoundRequestEvent { public string soundName; public bool isMusic; }

    // ========== VFX Events ==========
    public struct VFXRequestEvent { public string vfxName; public Vector3 position; }
}
