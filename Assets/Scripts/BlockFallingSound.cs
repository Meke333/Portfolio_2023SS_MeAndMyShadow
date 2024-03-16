using System;
using UnityEngine;

public class BlockFallingSound : MonoBehaviour
{
    private LayerMask layerToCheck;
    [SerializeField] internal BlockSoundType blockType;
    internal AudioSource blockAudioSource;

    //events
    public static event Action<BlockFallingSound> OnFallingBlock;
    private void Awake()
    {
        
        layerToCheck = LayerMask.GetMask("Default", "Interactable_Mid_Foreground");
        blockAudioSource = this.gameObject.AddComponent<AudioSource>();
    }

    private void OnCollisionEnter(Collision other)
    {
        if ((layerToCheck.value & (1 << other.gameObject.layer)) != 0)
        {
            blockAudioSource.volume = SoundManager.Instance.SFXVolume;
            
            //if wanted disable it, when in Selection Mode
            
            OnFallingBlock?.Invoke(this);
            
            //Debug.Log("ObjectSound");
        }
    }
}


