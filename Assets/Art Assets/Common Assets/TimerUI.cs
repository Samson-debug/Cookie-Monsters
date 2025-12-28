using System;
using UnityEngine;
using TMPro;
using System.Collections;
using CookieGame.Gameplay;
using UnityEngine.UI;
public class TimerUI : MonoBehaviour
{
     
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Image clockImage;
    [SerializeField] private int startTime=59;

    private int currentTime;
    private Coroutine _countdownCoroutine;

    private void Start()
    {
        currentTime = startTime;
        UpdateTimerText();
        // Timer will be started when gameplay state is entered
    }

    public void StartTimer()
    {
        if (_countdownCoroutine != null)
        {
            StopCoroutine(_countdownCoroutine);
        }
        currentTime = startTime;
        UpdateTimerText();
        _countdownCoroutine = StartCoroutine(StartCountDown());
    }

    public void StopTimer()
    {
        if (_countdownCoroutine != null)
        {
            StopCoroutine(_countdownCoroutine);
            _countdownCoroutine = null;
        }
    }

    private IEnumerator StartCountDown()
    {
        while (currentTime > 0)
        {
            yield return new WaitForSeconds(1f);
            currentTime--;
            UpdateTimerText();
        }
        
        TimerTimeEnd();
    }

    private void UpdateTimerText()
    {
        timerText.text = currentTime.ToString("00");
        
        //clockImage.fillAmount = (float)currentTime / (float)startTime;
    }

    private void TimerTimeEnd()
    {
        var gameplayManager = FindObjectOfType<GameplayController>();
        if (gameplayManager) gameplayManager.OnGameOver();
    }
}
