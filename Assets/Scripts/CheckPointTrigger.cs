using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CheckPointTrigger : MonoBehaviour
{
    public UnityEvent moveObjects;
    
    private BoxCollider _checkPointTrigger;
    [Tooltip("Please Number every Checkpoint in a chronological Order, according to which the Player arrives earlier")]
    [SerializeField] private int _thisCheckPointNumber;

    private void Awake()
    {
        _checkPointTrigger = GetComponent<BoxCollider>();
    }

    private void OnEnable()
    {
        GameManager.OnStartingWithCheckPoint += DeleteGameObject;
    }

    private void OnDisable()
    {
        GameManager.OnStartingWithCheckPoint -= DeleteGameObject;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            GameManager.Instance.ChangeLevelCheckpoint(this.gameObject.transform.position, _thisCheckPointNumber);
            Destroy(this.gameObject);
        }
    }

    void DeleteGameObject(Vector3 a, int savedCheckPointNr)
    {
        if (_thisCheckPointNumber < savedCheckPointNr)
        {
            Destroy(this.gameObject);
        }
        else if (_thisCheckPointNumber == savedCheckPointNr)
        {
            moveObjects.Invoke();
        }

    }


}
