using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class MoveObjectToNewPosition : MonoBehaviour
{
    private float _xCoordinate;
    private float _yCoordinate;

    private void Awake()
    {
        _xCoordinate = transform.position.x;
        _yCoordinate = transform.position.y;
    }

    public void SetXPos(float x)
    {
        _xCoordinate = x;
    }
    public void SetYPos(float y)
    {
        _yCoordinate = y;
    }
    
    public void MoveObjectToNewPositionMethod()
    {
        
        this.gameObject.transform.position = new Vector3(_xCoordinate, _yCoordinate, transform.position.z);
    }
}
