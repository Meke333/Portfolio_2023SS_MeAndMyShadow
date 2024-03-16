using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

//Observer of Game Manager
public class SoundManager : MonoBehaviour
{
    //Singleton Pattern
    public static SoundManager Instance;

    private AudioSource musicSource;
    private List<AudioSource> audioSourceList_sfx;
    private AudioSource mainSFXSource;
    [SerializeField] private AudioSource playerSource;

    private SoundLibrary soundLibrary;

    private MusicState currentMusicState;

    private float _musicVolume;
    private float _maxMusicVolume = 1;

    private float _maxSFXVolume = 1;

    public float MusicVolume
    {
        get { return _musicVolume; }
        set
        {
            if (value < 0)
                value = 0;
            else if (value > 1)
                value = 1;
            
            _musicVolume = value;
            musicSource.volume = _musicVolume;
        }
    }
    
    public float MaxMusicVolume
    {
        get { return _maxMusicVolume; }
        set
        {
            if (value < 0)
                value = 0;
            else if (value > 1)
                value = 1;
            
            _maxMusicVolume = value;
        }
    }
    
    public float SFXVolume
    {
        get { return _maxSFXVolume; }
        set
        {
            if (value < 0)
                value = 0;
            else if (value > 1)
                value = 1;
            
            _maxSFXVolume = value;
            mainSFXSource.volume = _maxSFXVolume;
            playerSource.volume = _maxSFXVolume;
            foreach (AudioSource a in audioSourceList_sfx)
            {
                a.volume = _maxMusicVolume;
            }
        }
    }
    
    private void Awake()
    {
        //Singleton Pattern
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        
        Instance = this;

        
        if (Camera.main != null)
        {
            musicSource = Camera.main.gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
        }
        
        mainSFXSource = Camera.main.gameObject.AddComponent<AudioSource>();

        if (GameObject.FindGameObjectWithTag("Player") != null)
            playerSource = GameObject.FindGameObjectWithTag("Player").AddComponent<AudioSource>();
        
        soundLibrary = GetComponent<SoundLibrary>();


        MusicVolume = MaxMusicVolume;
    }

    private void OnEnable()
    {
        //Action from GameManager
        GameManager.OnGameStateChange += GameState_Change;
        GameManager.OnMusicChange += ChangeMusic;
        FigureCharacterController.OnPlayerSFXPlaying += PlayPlayerSFX;
        MouseControls.OnMouseSFXPlaying += PlayMouseSFX;
        BlockFallingSound.OnFallingBlock += PlayFallenBlockSFX;

    }

    private void OnDisable()
    {
        //Action from GameManager
        GameManager.OnGameStateChange -= GameState_Change;
        GameManager.OnMusicChange -= ChangeMusic;
        FigureCharacterController.OnPlayerSFXPlaying -= PlayPlayerSFX;
        MouseControls.OnMouseSFXPlaying -= PlayMouseSFX;
        BlockFallingSound.OnFallingBlock -= PlayFallenBlockSFX;
    }

    private void GameState_Change(GameState gameState)
    {
        
        switch (gameState)
        {
            /*
            case GameState.MenuStart:
                ChangeMusic(MusicState.TitleScreen);
                break;*/
            case GameState.GameStart:
                SetMusicVolume(MaxMusicVolume);
                switch (GameManager.Instance.LevelNr)
                {
                    case -1:
                    case 0:
                        ChangeMusic(MusicState.TitleScreen);
                        break;
                    case 1:
                    case 2:
                    case 3:
                        ChangeMusic(MusicState.KidsRoom);
                        break;
                    case 4:
                    case 5: 
                    case 6:
                    case 7:
                        ChangeMusic(MusicState.LivingRoom);
                        break;
                    case 8:
                    case 9:
                    case 10:
                    case 11:
                        ChangeMusic(MusicState.Kitchen);   
                        break;
                }
                
                break;
            case GameState.MenuPause:
                SetMusicVolume(MusicVolume - 0.2f);
                break;
            case GameState.PlayerDead:
                SetMusicVolume(MusicVolume - 0.9f);
                break;
            case GameState.LevelReset:
                SetMusicVolume(0f);
                break;
            case GameState.LevelComplete:
                SetMusicVolume(MusicVolume - 0.2f);
                PlaySimpleSFX(soundLibrary.sfx_win1);
                
                break;
            case GameState.Winning:
                break;
        }
    }

    internal void ChangeMusic(MusicState musicState)
    {
        if (currentMusicState.Equals(musicState))
            return;

        currentMusicState = musicState;
        
        AudioClip newMusic = null;
        
        switch (musicState)
        {
            case MusicState.TitleScreen:
                newMusic = soundLibrary.music01_TitleScreen;
                break;
            case MusicState.KidsRoom:
                newMusic = soundLibrary.music02_KidRoom;
                break;
            case MusicState.LivingRoom:
                newMusic = soundLibrary.music03_LivingRoom;
                break;
            case MusicState.Kitchen:
                newMusic = soundLibrary.music04_KitchenRoom;
                break;
        }
        
            
        musicSource.clip = newMusic;
        musicSource.Play();
    }

    void SetMusicVolume(float newVolume)
    {
        MusicVolume = newVolume;
    }

    void PlayPlayerSFX(PlayerSoundState playerSFX)
    {
        AudioClip audioClip = null;
        switch (playerSFX)
        {
            case PlayerSoundState.Spawn:
                audioClip = soundLibrary.sfx_spawn;
                break;
            case PlayerSoundState.Step:
                break;
            case PlayerSoundState.Jump:
                audioClip = soundLibrary.sfx_jump;
                break;
            case PlayerSoundState.Land:
                audioClip = soundLibrary.sfx_land;
                break;
            case PlayerSoundState.Dead:
                audioClip = soundLibrary.sfx_death;
                break;
            default: 
                return;
        }
        playerSource.PlayOneShot(audioClip,_maxSFXVolume);
    }

    void PlayMouseSFX(MouseSoundState mouseSFX)
    {
        switch (mouseSFX)
        {
            case MouseSoundState.Click:
                break;
            case MouseSoundState.Select:
                break;
            case MouseSoundState.Unselect:
                break;
            case MouseSoundState.Rotate:
                break;
            case MouseSoundState.Scale:
                break;
            case MouseSoundState.Denied:
                break;
        }
    }

    void PlaySimpleSFX(AudioClip audioClip)
    {
        mainSFXSource.PlayOneShot(audioClip, _maxSFXVolume);
    }

    void PlayFallenBlockSFX(BlockFallingSound blockFallingSound)
    {
        AudioClip audioClip = null;
        
        switch (blockFallingSound.blockType)
        {
            case BlockSoundType.Wood:
                int i = Random.Range(0, soundLibrary.sfxWoodBlockLanding.Count);
                //Debug.Log(i);
                audioClip = soundLibrary.sfxWoodBlockLanding[i];
                break;
            case BlockSoundType.Plastic:
                break;
            case BlockSoundType.Glass:
                break;
            default:
                return;
        }
        blockFallingSound.blockAudioSource.clip = audioClip;
        blockFallingSound.blockAudioSource.Play();
    }


}

public enum MusicState
{
    TitleScreen,
    KidsRoom,
    LivingRoom,
    Kitchen
}
public enum PlayerSoundState
{
    Spawn,
    Step,
    Jump,
    Land,
    Dead,
}

public enum MouseSoundState
{
    Click,
    Select,
    Unselect,
    Rotate,
    Scale,
    Denied
}

public enum BlockSoundType
{
    Wood,
    Plastic,
    Glass
    
}

