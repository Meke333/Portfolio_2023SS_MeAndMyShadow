using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WinningUIScreenText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    
    private void OnEnable()
    {
        TimerScript.OnGameCompleteTimer += UpdateTimerText;
    }

    private void OnDisable()
    {
        TimerScript.OnGameCompleteTimer -= UpdateTimerText;
    }

    void UpdateTimerText(string newTime)
    {
        timerText.text = newTime;
    }
}
