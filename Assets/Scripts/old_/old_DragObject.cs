using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DragObject : MonoBehaviour
{
/*
    //Components
    private Rigidbody rigidbody;
    private BoxCollider boxcollider;

    //Rigidbody Attributes
    private float rb_mass;
    private Vector3 bc_center;
    private bool IsUsingGravity;
    
    //Mouse Attributes
    [SerializeField] private static bool _mouseHoldingObject; //For all Objects to prevent multiple holdings of Objects
    private Vector3 mouseOffset;
    private float mouseZCoordinate;

    //Testing: No Clipping in other Objects
    private bool isOnPositionCollision = false;
    private Vector3 directionAwayFromCollision;
    private float zPosition;

    [SerializeField] private GameObject borderLeft_gameObject;
    [SerializeField] private GameObject borderRight_gameObject;
    [SerializeField] private GameObject borderBottom_gameObject;
    [SerializeField] private GameObject borderUp_gameObject;

    private bool isMousePressed;

    [SerializeField] private float grabHeight = 2.5f;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        boxcollider = GetComponent<BoxCollider>();

        bc_center = boxcollider.center;
        
        rb_mass = rigidbody.mass;

        IsUsingGravity = rigidbody.useGravity;
        zPosition = transform.position.z;
    }

    //Calculate Offset between Mouse/Camera and Object
    private void OnMouseDown()
    {
        mouseZCoordinate = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;
        //Store offset = gameObject World pos - mouse world pos
        mouseOffset = gameObject.transform.position - GetMouseWorldPosition();

        rigidbody.mass = 0f;
        rigidbody.useGravity = false;
        boxcollider.center = boxcollider.center + new Vector3(0,0,-5);

        isMousePressed = true;
    }

    //Dragging the Object around the screen
    private void OnMouseDrag()
    {

            Vector3 v = GetMouseWorldPosition() + mouseOffset;

            if (v.y > grabHeight)
            {
                v.y = grabHeight;
            }
            //borders
            else if (v.y < borderBottom_gameObject.transform.position.y)
            {
                v.y = borderBottom_gameObject.transform.position.y;
            }

            if (v.x < borderLeft_gameObject.transform.position.x)
            {
                v.x = borderLeft_gameObject.transform.position.x;
            }
            else if (v.x > borderRight_gameObject.transform.position.x)
            {
                v.x = borderRight_gameObject.transform.position.x;
            }
            
        
            rigidbody.MovePosition(v); //GetMouseWorldPosition() + mouseOffset);

            

    }

    private void OnMouseUp()
    {
        rigidbody.mass = rb_mass;
        
        rigidbody.useGravity = IsUsingGravity ? true : false;
        
        boxcollider.center = bc_center;

        isMousePressed = false;
    }

    Vector3 GetMouseWorldPosition()
    {
        //pixel coordinate (x,y)
        Vector3 mousePoint = Input.mousePosition;
        
        //z coordinate of Game object on screen
        mousePoint.z = mouseZCoordinate;

        return Camera.main.ScreenToWorldPoint(mousePoint);
    }

    private void OnCollisionEnter(Collision other)
    {
        isOnPositionCollision = true;

        directionAwayFromCollision = transform.position + other.gameObject.transform.position;
    }

    private void OnCollisionExit(Collision other)
    {
        isOnPositionCollision = false;
    }
*/
    
}
