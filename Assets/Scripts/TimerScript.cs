using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerScript : MonoBehaviour
{
    private TimerScript Instance;
    
    private float startTime;
    private float endTime;

    private bool isBeginningFromStart;

    public static event Action<string> OnGameCompleteTimer;


    private void OnEnable()
    {
        GameManager.OnGameStateChange += GameStateCheck;
        PauseMenu.OnPauseMenuToGameMenu += ResetTimer;
        MainMenuScript.OnLevel1Start += SetStartTime;
    }

    private void OnDisable()
    {
        GameManager.OnGameStateChange -= GameStateCheck;
        PauseMenu.OnPauseMenuToGameMenu -= ResetTimer;
        MainMenuScript.OnLevel1Start -= SetStartTime;
    }

    private void Awake()
    {
        //Singleton Pattern
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        
        Instance = this;
    }

    void SetStartTime()
    {
        isBeginningFromStart = true;
        startTime = Time.unscaledTime;
        Debug.Log("starting Time: " + startTime);
    }

    void SetEndTime()
    {
        endTime = Time.unscaledTime;
        //Debug.Log("ending Time: " + startTime);
    }

    float CalculateDifferenceTime()
    {
        return endTime - startTime;
    }

    string GiveTimeInString()
    {
        if (!isBeginningFromStart)
            return "Beginn at Lvl 1!";

        float time = CalculateDifferenceTime();

        int minutes = (int) (time / 60f);
        int seconds = (int)(time % 60f);
        int fraction = (int)((time * 1000)%1000);
        
        return (string.Format("{0:00}:{1:00}.{2:000}", minutes, seconds, fraction));
    }

    void GameCompleteMenu()
    {
        SetEndTime();

        OnGameCompleteTimer?.Invoke(GiveTimeInString());
    }

    void ResetTimer()
    {
        Debug.Log("Timer RESET!!!");
        isBeginningFromStart = false;
        startTime = Time.unscaledTime;
    }

    void GameStateCheck(GameState gameState)
    {
        if (gameState != GameState.Winning)
            return;
        Debug.Log("TIMER ENDED!!!");
        GameCompleteMenu();
        
    }

}
