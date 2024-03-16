using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTriggerEvent : MonoBehaviour
{
    
    [SerializeField] internal ComparableTag comparableTag = ComparableTag.ShadowObjects;
    [SerializeField] internal FigureCharacterState triggeringState = FigureCharacterState.Push;

    [SerializeField] private Collider gameObjectCollider;

    public static event Action<bool> OnPushStateChange;

    private void OnEnable()
    {
        MouseControls.OnSelectedObject += SelectGameObject;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("A: Enter OnTrigger");
        if (other.gameObject.CompareTag(comparableTag.ToString()))
        {
            gameObjectCollider = other;
            OnPushStateChange?.Invoke(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Z: Exit OnTrigger");
        if (other.gameObject.CompareTag(comparableTag.ToString()))
        {
            
            if (other == gameObjectCollider)
            {
                gameObjectCollider = null;
                OnPushStateChange?.Invoke(false);
            }
            
        }
        
    }

    void SelectGameObject(GameObject g)
    {
        if (g.GetComponent<Collider>() != null)
            OnTriggerExit(g.GetComponent<Collider>());
        
    }
    
    
    
    
    
}
