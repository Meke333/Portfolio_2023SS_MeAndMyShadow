using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class anim : MonoBehaviour
{

    public GameObject camera;
    public GameObject soundManager;

    public GameObject[] GameObjects;

    void PanStarted()
    {
        GameManager.Instance.ChangeTimeScale(0f);
    }

    void SetTime(float a)
    {
        GameManager.Instance.ChangeTimeScale(a);
        AudioSource audio = GetComponent<AudioSource>();
        audio.clip = soundManager.GetComponent<SoundLibrary>().sfx_spawn;
        audio.Play();
    }
    
    
    void PanEnded()
    {
        camera.SetActive(true);
        soundManager.SetActive(true);
        this.gameObject.SetActive(false);
        foreach (GameObject g in GameObjects)
        {
            g.SetActive(true);
        }
        GameManager.Instance.ChangeTimeScale(1f);
        SoundManager.Instance.ChangeMusic(MusicState.KidsRoom);
    }
}
