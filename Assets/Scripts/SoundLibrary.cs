
using System.Collections.Generic;
using UnityEngine;

public class SoundLibrary : MonoBehaviour
{
    //Singleton
    public SoundLibrary Instance;
    
    #region Music
    
    public AudioClip music01_TitleScreen;
    public AudioClip music02_KidRoom;
    public AudioClip music03_LivingRoom;
    public AudioClip music04_KitchenRoom;
    
    #endregion

    #region SFX

    //Player
    public AudioClip sfx_spawn;
    public AudioClip sfx_jump;
    public AudioClip sfx_land;
    public AudioClip sfx_death;

    //Objects
    public List<AudioClip> sfxWoodBlockLanding;
    
    
    public AudioClip sfx_win1;

    #endregion

    private void Awake()
    {
        //Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
    }
}
