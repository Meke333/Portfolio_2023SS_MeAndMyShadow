
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu Instance;
    
    private PlayerInput input;

    private float pauseTimeScale;
    [SerializeField] private float originalGameTimeScale = 1;

    [SerializeField] private GameObject pauseMenuObject;
    [SerializeField] private GameObject winMenuObject;
    [SerializeField] private Animator levelNameAnimator;
    
    private static readonly int ShowName = Animator.StringToHash("showName");

    public static event Action OnPauseMenuToGameMenu;


    private void OnEnable()
    {
        input = new PlayerInput();
        
        GameManager.OnGameStateChange += PauseMenuInitiated;
        GameManager.OnFirstTimeLevelStart += ShowLevelName;

        input.PauseMenu.Enable();
        
        input.PauseMenu.TogglePauseMenu.performed += PauseMenuToggleInitiation;
    }

    private void OnDisable()
    {
        GameManager.OnGameStateChange -= PauseMenuInitiated;
        GameManager.OnFirstTimeLevelStart -= ShowLevelName;
        
        input.PauseMenu.Disable();
    }

    void PauseMenuInitiated(GameState gameState)
    {
        if (gameState == GameState.MenuPause)
        {
            Debug.Log("Game Paused");
            originalGameTimeScale = GameManager.Instance.GameTimeScale;
            
            GameManager.Instance.ChangeTimeScale(pauseTimeScale);
        }
        else if (gameState == GameState.GameStart)
        {
            GameManager.Instance.ChangeTimeScale(originalGameTimeScale);
        }
        else if (gameState == GameState.LevelComplete)
        {
            //Set Animation in here to make a fancy Win Screen
            winMenuObject.SetActive(true);
        }
        else if (gameState == GameState.Winning)
        {
            GameManager.Instance.GameState_Change(GameState.LevelComplete);
        }
    }
    
    //InputSystem
    void PauseMenuToggleInitiation(InputAction.CallbackContext context)
    {
        TogglePauseMenu();
    }

    public void TogglePauseMenu()
    {
        Debug.Log("PauseMenu Pressed");
        if (GameManager.Instance.ManagerGameState == GameState.GameStart)
        {
            GameManager.Instance.GameState_Change(GameState.MenuPause);
            pauseMenuObject.SetActive(true);
            
        }
        
        else if (GameManager.Instance.ManagerGameState == GameState.MenuPause)
        {
            GameManager.Instance.GameState_Change(GameState.GameStart);
            pauseMenuObject.SetActive(false);
        }
    }

    public void ResumePauseMenu()
    {
        if (GameManager.Instance.ManagerGameState == GameState.MenuPause)
        {
            GameManager.Instance.GameState_Change(GameState.GameStart);
            pauseMenuObject.SetActive(false);
        }
        
    }

    public void RestartPauseMenu()
    {
        GameManager.Instance.GameState_Change(GameState.LevelReset);
    }

    public void NextLevel()
    {
        GameManager.Instance.GameState_Change(GameState.NextLevel);
    }

    public void GoToMenu()
    {
        
        GameManager.Instance.ChangeLevelNr(0);
        GameManager.Instance.GameState_Change(GameState.LevelLoading);
        OnPauseMenuToGameMenu?.Invoke();
    }

    void ShowLevelName()
    {
        levelNameAnimator.SetTrigger(ShowName);
    }
    
}
