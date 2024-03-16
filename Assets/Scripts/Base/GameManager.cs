using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    //Singleton Pattern
    public static GameManager Instance;
    
    [SerializeField] private GameState gameState;

    public GameState ManagerGameState
    {
        get { return gameState; }
    }
    
    //Attributes
    [SerializeField] private int _levelNr;
    private Vector3 _currentCheckPointPosition;
    private int _currentOfCheckPointNr;

    public int LevelNr
    {
        get
        {
            return _levelNr;
        }
    }
    
    private bool isLevelResetAlreadyOngoing;
    
    //Time
    private float gameTimeScale;

    public float GameTimeScale
    {
        get { return gameTimeScale; }
        set
        {
            if (value < 0)
                value = 0;

            gameTimeScale = value;
        }
    }
    
    //Events
    public static event Action<GameState> OnGameStateChange; //For Observers
    public static event Action<MusicState> OnMusicChange;
    public static event Action OnPlayerDied;
    public static event Action OnFirstTimeLevelStart;
    public static event Action<Vector3, int> OnStartingWithCheckPoint;
    
    private void Awake()
    {
        //Singleton Pattern
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        
        Instance = this;
        
        //Exchangeable between Scenes
        DontDestroyOnLoad(this);
        
        
    }

    private void Start()
    {
        //CHANGE IT TO THE FIRST GAME STATE
        GameState_Change(GameState.GameStart);
        Debug.Log("Game Start initialized");
    }

    public void GameState_Change(GameState gs)
    {
        
        
        //Copy to Observers & only choose the States, which are needed
        switch (gs)
        {
            /*case GameState.MenuStart:
                Debug.Log("Menu");
                break;*/
            case GameState.LevelLoading:
                StartCoroutine(LoadLevel(_levelNr, false));
                break;
            case GameState.GameStart:
                break;
            case GameState.MenuPause:
                break;
            case GameState.PlayerDead:
                Debug.Log("PlayerDead GameState");
                OnPlayerDied?.Invoke();
                
                if (!isLevelResetAlreadyOngoing)
                    StartCoroutine(LoadLevel(SceneManager.GetActiveScene().buildIndex, true));
                break;
            case GameState.LevelReset:
                if (!isLevelResetAlreadyOngoing)
                {
                    _currentCheckPointPosition = Vector3.zero;
                    StartCoroutine(LoadLevel(SceneManager.GetActiveScene().buildIndex, false));
                }
                break;
            case GameState.LevelComplete:
                //Debug.Log("LevelComplete");
                LevelCompletion();
                break;
            case GameState.NextLevel:
                if (!isLevelResetAlreadyOngoing)
                    StartCoroutine(LoadLevel(SceneManager.GetActiveScene().buildIndex + 1, false));
                break;
            case GameState.Winning:
                break;
        }
        
        gameState = gs;
        
        //Notify Observers with the new GameState to work with it
        OnGameStateChange?.Invoke(gameState);
    }

    void PlayerDied()
    {
        
    }

    IEnumerator LoadLevel(int levelNr, bool isPlayerDead)
    {
        isLevelResetAlreadyOngoing = true;
        if (isPlayerDead)
        {
            ChangeTimeScale(0f);
            yield return new WaitForSecondsRealtime(0.5f);
            ChangeTimeScale(1f);
            yield return new WaitForSeconds(1f);
            
        }
        
        //Load LevelNr
        _levelNr = levelNr;
        ChangeTimeScale(1f);
        SceneManager.LoadScene(_levelNr, LoadSceneMode.Single);

        yield return new WaitForSeconds(0.05f);
        
        if (isPlayerDead)
        {
            if (_currentCheckPointPosition != Vector3.zero)
                OnStartingWithCheckPoint?.Invoke(_currentCheckPointPosition, _currentOfCheckPointNr);
        }
        
        
        yield return new WaitForSeconds(1.25f);
        
        Debug.Log("Hello Im Finished Restarting");
        
        //if (levelNr == 1) 
            //yield return new WaitForSeconds(1.25f);
        
        if (!isPlayerDead)
            OnFirstTimeLevelStart?.Invoke();
        
        GameState_Change(GameState.GameStart);
        isLevelResetAlreadyOngoing = false;

        
        

    }
    void LevelCompletion()
    {
        _currentCheckPointPosition = Vector3.zero;
    }

    public void ChangeTimeScale(float _timeScale)
    {
        GameTimeScale = _timeScale;
        Time.timeScale = GameTimeScale;
    }

    public void ChangeLevelNr(int newLevelNr)
    {
        //Debug.Log(newLevelNr);
        _levelNr = newLevelNr;
    }

    public void ChangeLevelCheckpoint(Vector3 checkPointPosition, int checkPointNumber)
    {
        _currentOfCheckPointNr = checkPointNumber;
        _currentCheckPointPosition = checkPointPosition;
    }

}



