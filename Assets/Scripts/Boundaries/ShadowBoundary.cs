using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowBoundary : MonoBehaviour
{
    [SerializeField] private GameObject zpos;
    [SerializeField] private LayerMask shadowInteractibleLayer;

    private void OnTriggerEnter(Collider other)
    { 
        if ((shadowInteractibleLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            other.transform.position = zpos.transform.position;
        }
    }
}
