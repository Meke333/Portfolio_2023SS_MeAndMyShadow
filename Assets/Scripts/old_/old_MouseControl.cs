using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Scripting;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class MouseControl : MonoBehaviour
{
/*
    //selected Object will be manipulated in this script
    [SerializeField] private GameObject selectedObjectShadow;       //Parent Object
    [SerializeField] private GameObject selectedObjectForeground;   //Child Object
    [SerializeField] private GameObject selectedObjectIndicator;    //Child-Child Object
    [SerializeField] private GameObject selectedObjectSelectable;    //Dragable Object
    [SerializeField] private GameObject temporaryParentObject;      //temporary Parent Object
    //private Rigidbody rigidbody_selectedObjectForeground;
    [SerializeField] private ObjectRelationInformation ori;
    
    //Unity Objects
    [SerializeField] private Ray mouseRay;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask clickableLayermask;
    
    
    [SerializeField] private bool _isInSelection;
    
    //calculations
    private Vector3 mouseOffset;
    private float mouseZCoordinate;

    //InputSystem
    private PlayerInput input;

    private bool input_isScaling;
    private bool input_isRotating;
    private float scaledirection;
    public float scaleSensitivity;
    [SerializeField] private float scaleIntensity; //0.075 is the limit!!
    private Vector2 rotation_direction;
    private Vector2 rotation_point;
    [SerializeField] private Vector2 screenMouseCoordinate; //Putting the Coordination System from left up (0,0) to middle

    public static event Action<MouseAction> OnMouseEvent;

    private void OnEnable()
    {
        input.MouseControls.Enable();
    }
    
    private void OnDisable()
    {
        input.MouseControls.Disable();
    }

    private void Awake()
    {

        mainCamera = Camera.main;
        
        input = new PlayerInput();

        input.MouseControls.Mouse_Click.performed += SelectObject;
        input.MouseControls.Mouse_Scale.started += ScalePressed;
        input.MouseControls.Mouse_Scale.canceled += ScalePressed;
        input.MouseControls.Mouse_Rotate.started += RotateInitiated;
        input.MouseControls.Mouse_Rotate.canceled += RotateReleased;
        
    }

    private void FixedUpdate()
    {
    
        if (!_isInSelection)
            return;
        
        //Check if no other Input is selected
        if (!input_isRotating && !input_isScaling)
            MoveObject();
    }

    //InputSystem methods
    void SelectObject(InputAction.CallbackContext context)
    {
        //Check if a Object is already selected!
        if (_isInSelection)
        {
            UnselectObject();
            return;
        }
        
        
        //Cast Ray based on Mouse Position
        mouseRay = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit _raycastHit;

        //Check Ray if a Child-GameObject is on Mouse
        if (Physics.Raycast(mouseRay,out _raycastHit,100, clickableLayermask))
            //If true: Then Invoke GetSelectedObject with the Child-GameObject from the Ray
        {
            //Event: OnMouseEvent
            OnMouseEvent?.Invoke(MouseAction.Click_Select);
            
            _isInSelection = true;
            
            GetSelectedObject(_raycastHit.transform.gameObject); 
            
            //Disable rigidbody
            //rigidbody_selectedObjectForeground.isKinematic = true;

            //Parent Child-Object to temporary Object
            selectedObjectForeground.transform.SetParent(temporaryParentObject.transform);
            selectedObjectSelectable.transform.SetParent(temporaryParentObject.transform);
            
            //Disable Graphics & Collision of Parent-Object & Enable Graphics of Child-Child-Object 
            ToggleActivationOfGameObject(selectedObjectShadow, false);
            ToggleActivationOfGameObject(selectedObjectIndicator, true);
            //Disable Dragable
            ToggleActivationOfGameObject(selectedObjectSelectable, false);
            
            //Mouse Calculations
            Vector3 position = selectedObjectForeground.transform.position;
            mouseZCoordinate = mainCamera.WorldToScreenPoint(position).z;
            
            //Store offset = gameObject World pos - mouse world pos
            mouseOffset = position - GetMouseWorldPosition();
        }
        else
        {
            //Event: OnMouseEvent
            OnMouseEvent?.Invoke(MouseAction.Click_Nothing);
        }
        
        //If false: Maybe Feedback that GameObject is not selected
        
    }

    void ScalePressed(InputAction.CallbackContext context)
    {
        if (!_isInSelection)
            return;
        //Event: OnMouseEvent
        OnMouseEvent?.Invoke(MouseAction.Scale);
        
        scaledirection = Mathf.Clamp(context.ReadValue<float>(), -1f, 1f);
        input_isScaling = scaledirection != 0;


        ScaleObject(scaledirection);
        
        //Debug.Log(scaledirection);
    }

    void RotateInitiated(InputAction.CallbackContext context)
    {
        if (!_isInSelection)
            return;

        rotation_point = (Vector2)Input.mousePosition;
        screenMouseCoordinate = rotation_point;
        //Debug.Log(rotation_point);
        input_isRotating = true;
    }

    void RotateReleased(InputAction.CallbackContext context)
    {
        if (!_isInSelection)
            return;
        
        //Event: OnMouseEvent
        OnMouseEvent?.Invoke(MouseAction.Rotate);
        
        rotation_direction = (Vector2) Input.mousePosition;
        //Calculation of which Rotation is going to initialized
        Vector2 rotation_calculated = rotation_direction - rotation_point;
        
        //Debug.Log(rotation_calculated.normalized);
        
        //Rotate with calculated direction
        RotateObject(rotation_calculated);
        input_isRotating = false;
    }
    
    void UnselectObject()
    {
        //Event: OnMouseEvent
        OnMouseEvent?.Invoke(MouseAction.Click_Unselect);
        
        //Update Parent-Object
        UpdateObjectTransform(selectedObjectShadow, selectedObjectIndicator, true);
        UpdateObjectTransform(selectedObjectSelectable, selectedObjectForeground, false);
        
        //Reparent to Original Parent
        selectedObjectSelectable.transform.SetParent(selectedObjectShadow.transform);
        selectedObjectForeground.transform.SetParent(selectedObjectShadow.transform);

        //Disable Graphics of Child-Child-Object & Enable Graphics & Collision of Parent-Object
        ToggleActivationOfGameObject(selectedObjectIndicator, false);
        ToggleActivationOfGameObject(selectedObjectShadow, true);
        
        //Enable Dragable
        ToggleActivationOfGameObject(selectedObjectForeground, false);
        ToggleActivationOfGameObject(selectedObjectSelectable, true);
        
        
        //Enable Rigidbody
        //rigidbody_selectedObjectForeground.isKinematic = false;

        //Erase References
        selectedObjectShadow = null;
        selectedObjectForeground = null;
        selectedObjectIndicator = null;
        _isInSelection = false;
    }
    
    //Control Methods
    void MoveObject()
    {
        //Implement the Move-Method from DragObject for the Child-Object
        Vector3 v = GetMouseWorldPosition() + mouseOffset;
        
        //!!!!!!!!!!!!!!Use Observer Pattern, when the Border moves then update it
        float grabheight = BorderInformation.Instance.grabHeight;
        float borderup = BorderInformation.Instance.borderUp.transform.position.y;
        float borderdown = BorderInformation.Instance.borderBottom.transform.position.y;
        float borderleft = BorderInformation.Instance.borderLeft.transform.position.x;
        float borderright = BorderInformation.Instance.borderRight.transform.position.x;
        //float borderfront = BorderInformation.Instance.borderFront.transform.position.z;
        //float borderback = BorderInformation.Instance.borderBack.transform.position.z;
        
        if (v.y > grabheight && grabheight != 0 && grabheight < borderup)
        {
            v.y = grabheight;
        }
        //borders
        else if (v.y > borderup)
        {
            v.y = borderup;
        }
        else if (v.y < borderdown)
        {
            v.y = borderdown;
        }
        
        if (v.x < borderleft)
        {
            v.x = borderleft;
        }
        else if (v.x > borderright)
        {
            v.x = borderright;
        }

        /* if (v.z < borderfront)
        {
            v.z = borderfront;
        }
        else if (v.z > borderback)
        {
            v.z = borderback;
        }
        */
/*

        selectedObjectForeground.transform.position = v;
        //rigidbody_selectedObjectForeground.MovePosition(v); //GetMouseWorldPosition() + mouseOffset);

    }

    void ScaleObject(float direction)
    {
        //Check if Forward or Backwards
        if (direction == 0)
            return;
        
        Vector3 TransformInWorldSpace = selectedObjectForeground.transform.TransformPoint(Vector3.zero);

        if ((TransformInWorldSpace.z <= BorderInformation.Instance.borderFront.transform.position.z && direction < 0) ||
            (TransformInWorldSpace.z >= BorderInformation.Instance.borderBack.transform.position.z && direction > 0))
            return;
        

        /*Vector3 TransformInLocalSpace = selectedObjectForeground.transform.InverseTransformPoint(Vector3.forward);
        selectedObjectForeground.transform.Translate(TransformInLocalSpace * direction, Space.World);

        Debug.Log(TransformInLocalSpace);
        Debug.Log(mouseOffset);
        */
/*
        //Move Foreground Object 
        mouseOffset += (Vector3.forward * direction * scaleSensitivity);

        //Move Shadow Object to remain in place
        Vector3 indicatorPosition = selectedObjectIndicator.transform.position;
        indicatorPosition -= (Vector3.forward * direction * scaleSensitivity);
        
        //Implement the Scale-Method:
        selectedObjectIndicator.transform.localScale -= (Vector3.one * direction * scaleIntensity);
        indicatorPosition -= ((Vector3.up * direction * (scaleIntensity/2)));
        
        
        selectedObjectIndicator.transform.position = indicatorPosition;

        //Reparent Child-Object to temporary Object
        //Move Child-Object
        //Scale Child-Child-Object
        //Recalculate Position 

    }

    void RotateObject(Vector2 direction)
    {
        
        //Check in which Direction it should rotate
        Vector3 vector3_rotation_direction = GetRotationDirection(direction);

        //If direction is 0 or unclear, then cancel method
        if (vector3_rotation_direction.Equals(Vector3.zero))
        {
            return;
        }
        
        Debug.Log("Rotating Objects");
        
        //Disable Graphics of Child-Child-Object
        ToggleActivationOfGameObject(selectedObjectIndicator, false);
        
        //Rotate Child-Object
        selectedObjectForeground.transform.Rotate(vector3_rotation_direction,90f,Space.World);

        //Update Child-Child-Object
        selectedObjectIndicator.transform.RotateAround(selectedObjectForeground.transform.position,vector3_rotation_direction,-90f);
        selectedObjectIndicator.transform.Rotate(vector3_rotation_direction,90f,Space.World);

        //Update Parent-Object

        //Enable Graphics of Child-Child-Object
        ToggleActivationOfGameObject(selectedObjectIndicator, true);
    }
    
    //Helper Methods
    void ToggleActivationOfGameObject(GameObject gameObject, bool isSetToActive)
    {
        gameObject.SetActive(isSetToActive);
    }

    void UpdateObjectTransform(GameObject edit_object, GameObject target_object, bool isScaleImportant)
    {
        //Position
        edit_object.transform.position = target_object.transform.position;
        
        //Rotation
        edit_object.transform.rotation = target_object.transform.rotation;

        //Scale
        if (isScaleImportant)
        {
            /*Vector3 original = edit_object.transform.localScale;
            Vector3 target = target_object.transform.localScale;
            edit_object.transform.localScale =  new Vector3(original.x * target.x, original.y * target.y, original.z * target.z);
            */
/*
            edit_object.transform.localScale = target_object.transform.localScale;
            
        }
        
    }
    
    void GetSelectedObject(GameObject newSelectedObject)
    {
        selectedObjectSelectable = newSelectedObject.transform.GetChild(1).gameObject;//Raycast will always hit the parent Object... therefore this workaround.
        ori = newSelectedObject.GetComponentInChildren<ObjectRelationInformation>();
        
        ToggleActivationOfGameObject(ori.Dragable, true);
        
        //Get all other Objects
        selectedObjectForeground = ori.Dragable;
        selectedObjectShadow = ori.Shadow;
        selectedObjectIndicator = ori.Indicator;
    }
    
    Vector3 GetMouseWorldPosition()
    {
        //pixel coordinate (x,y)
        Vector3 mousePoint = Input.mousePosition; //Mouse.current.position.ReadValue();
        
        //z coordinate of Game object on screen
        mousePoint.z = mouseZCoordinate;

        return mainCamera.ScreenToWorldPoint(mousePoint);
    }
    
    Vector3 GetRotationDirection(Vector2 direction)
    {
        Vector3 result_direction;
        direction = direction.normalized;
        
        Debug.Log(direction);
        //Right
        if (direction.x > 0.8f && Mathf.Abs(direction.y) < 0.7f)
        {
            result_direction = Vector3.down;
        }
        //Left
        else if (direction.x < -0.8f && Mathf.Abs(direction.y) < 0.7f)
        {
            result_direction = Vector3.down;
        }
        //Up
        else if (direction.y > 0.8f && Mathf.Abs(direction.x) < 0.7f)
        {
            result_direction = Vector3.right;
        }
        //Down
        else if (direction.y < -0.8f && Mathf.Abs(direction.x) < 0.7f)
        {
            result_direction = Vector3.left;
        }
        else
        {
            result_direction = Vector3.zero;
        }
        
        //Debug
        Debug.Log(result_direction);

        return result_direction;

    }
    */
}
