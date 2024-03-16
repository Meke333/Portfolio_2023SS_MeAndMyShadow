using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BorderInformation : MonoBehaviour
{
    public static BorderInformation Instance;
    
    [SerializeField] internal GameObject borderLeft;
    [SerializeField] internal GameObject borderRight;
    [SerializeField] internal GameObject borderBottom;
    [SerializeField] internal GameObject borderUp;
    [SerializeField] internal GameObject borderFront;
    [SerializeField] internal GameObject borderBack;
    [SerializeField] internal GameObject grabHeight;
    [SerializeField] internal GameObject leftSelectedMoveBarrier;
    [SerializeField] internal GameObject rightSelectedMoveBarrier;

    private void Awake()
    {
        //SingletonPattern
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        
        Instance = this;
        
        //Get Child Objects of the Borders
        borderLeft = this.transform.GetChild(0).gameObject;
        borderRight = this.transform.GetChild(1).gameObject;
        borderBottom = this.transform.GetChild(2).gameObject;
        borderUp = this.transform.GetChild(3).gameObject;
        borderFront = this.transform.GetChild(4).gameObject;
        borderBack = this.transform.GetChild(5).gameObject;
        grabHeight = this.transform.GetChild(6).gameObject;
        
        leftSelectedMoveBarrier = GameObject.Find("LeftMoveBorder");
        rightSelectedMoveBarrier = GameObject.Find("RightMoveBorder");
    }
}
