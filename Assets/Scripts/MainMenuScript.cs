
using System;
using TMPro;
using UnityEngine;

public class MainMenuScript : MonoBehaviour
{
    private MenuState _menuState;

    #region variables
    [TextArea]
    [SerializeField] private string[] levelNames = new string[11];
    private int levelNr = 1; //0-9

    #endregion

    #region Buttons


    #endregion

    #region GameObjects

    [SerializeField] private GameObject titleScreen;
    [SerializeField] private GameObject helpScreen;
    [SerializeField] private GameObject levelSelectScreen;

    [SerializeField] private GameObject levelSelect_Page1;
    [SerializeField] private GameObject levelSelect_Page2;
    [SerializeField] private GameObject levelSelect_Page3;

    #endregion

    [SerializeField] private TextMeshProUGUI LevelNameText;

    public static event Action OnLevel1Start; 

    public void CyclePage(int page)
    {
        switch (page)
        {
            case 1:
                levelSelect_Page1.SetActive(true);
                levelSelect_Page2.SetActive(false);
                levelSelect_Page3.SetActive(false);
                break;
            case 2:
                levelSelect_Page1.SetActive(false);
                levelSelect_Page2.SetActive(true);
                levelSelect_Page3.SetActive(false);
                break;
            case 3:
                levelSelect_Page1.SetActive(false);
                levelSelect_Page2.SetActive(false);
                levelSelect_Page3.SetActive(true);
                break;
        }
    }
    
    public void UpdateLevelName(int nr)
    {
        levelNr = nr-1;
        string name;

        if (levelNr < 0 || levelNr > levelNames.Length - 1)
            name = "";
        
        else
            name = levelNames[levelNr];
        
        LevelNameText.text = name;
    }

    public void LevelLoad(int nr)
    {
        if (nr == 1)
            OnLevel1Start?.Invoke();
        
        GameManager.Instance.ChangeLevelNr(nr);
        GameManager.Instance.GameState_Change(GameState.LevelLoading);
    }

    public void GameStart()
    {
        levelNr = 1;
        OnLevel1Start?.Invoke();
        LevelLoad(levelNr);
    }

    public void GoBackToTitleScreen()
    {
        titleScreen.SetActive(true);
        helpScreen.SetActive(false);
        levelSelectScreen.SetActive(false);
    }

    public void GoToLevelSelect()
    {
        CyclePage(1);
        titleScreen.SetActive(false);
        helpScreen.SetActive(false);
        levelSelectScreen.SetActive(true);
    }

    public void GoToHelpScreen()
    {
        titleScreen.SetActive(false);
        helpScreen.SetActive(true);
        levelSelectScreen.SetActive(false);
    }

    public void ExitProgram()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
        Application.Quit();
    }
}

public enum MenuState
{
    TitleScreen,
    StartGame,
    LevelSelect,
    Help
}

[System.Serializable]
[CreateAssetMenu(menuName = "LevelDescription")]
public class LevelDescription : ScriptableObject
{
    public string name;
}


