using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectedObjectTrigger : MonoBehaviour
{
    private Collider _collider;
    private bool isSelected;

    public static event Action<bool> OnCollidingWithPlayer;
    private void Awake()
    {
        _collider.GetComponent<Collider>();
    }

    private void OnEnable()
    {
        MouseControls.OnMouseEvent += MouseActionChange;
    }

    private void OnDisable()
    {
        MouseControls.OnMouseEvent -= MouseActionChange;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isSelected || !other.CompareTag("Player"))
            return;
        
        Debug.Log("Trigger Entered");
        OnCollidingWithPlayer?.Invoke(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!isSelected || !other.CompareTag("Player"))
            return;
            
        Debug.Log("Trigger Exited");
        OnCollidingWithPlayer?.Invoke(false);
    }

    void MouseActionChange(MouseAction a)
    {
        if (a == MouseAction.Unselected)
        {
            isSelected = false;
        }
        else if (a == MouseAction.Selected)
        {
            isSelected = true;
        }
    }
    
    
}
