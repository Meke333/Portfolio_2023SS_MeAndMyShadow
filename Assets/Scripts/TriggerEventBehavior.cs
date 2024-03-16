using UnityEngine;

public class TriggerEventBehavior : MonoBehaviour
{
    //This script will trigger an GameManager Event
    
    [SerializeField] internal GameState triggeringGameState;
    [SerializeField] internal ComparableTag comparableTag;
    [SerializeField] internal bool loopableTrigger;

    private bool isalreadyTriggered;

    private bool IsAlreadyTriggered
    {
        get
        {
            return isalreadyTriggered;
        }
        set
        {
            isalreadyTriggered = !loopableTrigger ? value : false;
        }
    }
    
    
    private void OnTriggerEnter(Collider other)
    {
        if (IsAlreadyTriggered)
            return;
        
        if (other.gameObject.CompareTag(comparableTag.ToString()))
        {
            IsAlreadyTriggered = true;
            GameManager.Instance.GameState_Change(triggeringGameState);
        }
    }
    
    private void OnCollisionEnter(Collision other)
    {
        if (IsAlreadyTriggered)
            return;
        
        if (other.gameObject.CompareTag(comparableTag.ToString()))
        {
            IsAlreadyTriggered = true;
            GameManager.Instance.GameState_Change(triggeringGameState);
        }
    }
}

public enum ComparableTag
{
    Player,
    Goal,
    ShadowObjects,
        
}