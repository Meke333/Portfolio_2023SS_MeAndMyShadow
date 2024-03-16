using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

public class ObjectRelationInformation : MonoBehaviour
{
    public GameObject Shadow;       //Parent
    public GameObject Dragable;     //Child
    public GameObject Indicator;    //Child-Child

    private void Awake()
    {
        //Automatic Shadow getting
        Shadow = this.transform.parent.gameObject;
    }
}
