using System;
using System.Collections;
using System.Numerics;
using UnityEngine;
using UnityEngine.InputSystem;
using Quaternion = System.Numerics.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class MouseControls : MonoBehaviour
{
    #region GameObjects for Interactible Objects

    [SerializeField] private GameObject foregroundGameObject;
    [SerializeField] private GameObject shadowGameObject;
    [SerializeField] private GameObject indicatorGameObject;
    [SerializeField] private GameObject emptyParentGameObject;
    
    //[SerializeField] private GameObject lightsourcePosition; //for calculating Distance between Objects and lightSource Object: Specifically the z-Coordinate

    #endregion
    
    #region Rigidbody of foregroundGameObject
    
    [SerializeField] private Rigidbody foregroundRigidbody;
    [SerializeField] private Collider foregroundCollider;
    [SerializeField] private float foregroundMass;
    private Vector3 foregroundVelocity;

    #endregion

    #region Unity Objects
    
    private Ray mouseRay;
    private Camera mainCamera;
    [SerializeField] private LayerMask clickableLayerMask;

    #endregion

    #region Calculations
    
    private Vector3 mouseOffset;
    private float mouseZCoordinate;
    
    #endregion
    
    #region InputSystem

    private PlayerInput input;

    private MouseAction _mouseActionState;
    private bool isStillRotating;
    private float scaledirection;
    public float scaleSensitivity;

    [SerializeField] private float scaleIntensity; //0.075 is the limit!!
    [SerializeField] private float massIntensity;
    private Vector2 rotation_direction;
    private Vector2 rotation_point;
    
    private Vector2 screenMouseCoordinate; //Putting the Coordination System from left up (0,0) to middle


    #endregion
    
    #region Variables

    [SerializeField] private bool canMoveCursor;

    private bool isPlayerCollidingOnSelection;

    #endregion
    
    
    //Events
    public static event Action<MouseAction> OnMouseEvent;
    public static event Action<MouseSoundState> OnMouseSFXPlaying;
    public static event Action<GameObject> OnSelectedObject;

    private void OnEnable()
    {
        _mouseActionState = MouseAction.Unselected;
        
        GameManager.OnGameStateChange += GameStateChanging;
        SelectedObjectTrigger.OnCollidingWithPlayer += CollidingWithPlayer;
        
        mainCamera = Camera.main;
        
        input = new PlayerInput();
        
        input.MouseControls.Enable();

        input.MouseControls.Mouse_Click.performed += SelectObject;
        input.MouseControls.Mouse_Scale.started += ScalePressed;
        input.MouseControls.Mouse_Scale.canceled += ScalePressed;
        input.MouseControls.Mouse_Rotate.started += RotateInitiated;
        input.MouseControls.Mouse_Rotate.canceled += RotateReleased;
    }

    private void OnDisable()
    {
        GameManager.OnGameStateChange -= GameStateChanging;
        SelectedObjectTrigger.OnCollidingWithPlayer -= CollidingWithPlayer;
        
        input.MouseControls.Disable();
        
        input.MouseControls.Mouse_Click.performed -= SelectObject;
        input.MouseControls.Mouse_Scale.started -= ScalePressed;
        input.MouseControls.Mouse_Scale.canceled -= ScalePressed;
        input.MouseControls.Mouse_Rotate.started -= RotateInitiated;
        input.MouseControls.Mouse_Rotate.canceled -= RotateReleased;
    }

    private void FixedUpdate()
    {
        if (canMoveCursor && Input.GetKey(KeyCode.R))
        {
            GameManager.Instance.GameState_Change(GameState.PlayerDead);
        }

        if (!canMoveCursor)
            return;
        
        if (_mouseActionState == MouseAction.Unselected)
            return;

        //Check if no other Input is selected
        if (_mouseActionState == MouseAction.Selected)
            MoveObject();
    }

    //InputSystem methods
    void SelectObject(InputAction.CallbackContext context)
    {
        if (!canMoveCursor)
        {
            Debug.Log("canceled due to !canMoveCursor");
            return;
        }

        if (isStillRotating)
            return;
        
        //Check if a Object is already selected!
        if (_mouseActionState == MouseAction.Selected)
        {  
            if (!isPlayerCollidingOnSelection)
                UnselectObject();
            
            return;
        }

        //Cast Ray based on Mouse Position

        //if there is a grabBarrier then check if the MouseClick is above the grabBarrier, if so then dont select anything
        float grabBarrier = mainCamera.WorldToScreenPoint(BorderInformation.Instance.grabHeight.transform.position).y;
        float mousePos = Input.mousePosition.y;
        
        /*
        Debug.Log("grabBarrier = " + grabBarrier);
        Debug.Log("mousePos = " + mousePos);
        */
        if (mousePos > grabBarrier)
        {
            ChangeMouseState(MouseAction.Unselected);
            Debug.Log("canceled due to GrabBarrier");
            return;
        }
        
        mouseRay = mainCamera.ScreenPointToRay(Input.mousePosition);
        
        RaycastHit _raycastHit;

        //Check Ray if a Child-GameObject is on Mouse
        if (Physics.Raycast(mouseRay, out _raycastHit, 100, clickableLayerMask))
            //If true: Then Invoke GetSelectedObject with the Child-GameObject from the Ray
        {
            
            //Event: OnMouseEvent
            ChangeMouseState(MouseAction.Selected);

            GetSelectedObject(_raycastHit.transform.gameObject); 
            //Disable rigidbody
            foregroundRigidbody.useGravity = false;
            foregroundVelocity = foregroundRigidbody.velocity;
            foregroundRigidbody.velocity = Vector3.zero;
            
            //Graphics & Collision: Shadow disabled, Indicator enabled
            
            shadowGameObject.transform.position =  Vector3.up * 500;
            ToggleActivationOfGameObject(shadowGameObject, false);
            
            shadowGameObject.transform.position = Vector3.zero;
            ToggleActivationOfGameObject(indicatorGameObject, true);
            
            //Mouse Calculations
            Vector3 position = foregroundGameObject.transform.position;
            mouseZCoordinate = mainCamera.WorldToScreenPoint(position).z;
            
            //Store offset = gameObject World pos - mouse world pos
            mouseOffset = position - GetMouseWorldPosition();
        }
        else
        {
            //Event: OnMouseEvent
            ChangeMouseState(MouseAction.Unselected);
        }
        
    }
    
    void ScalePressed(InputAction.CallbackContext context)
    {
        if (!canMoveCursor)
            return;
        
        if (_mouseActionState == MouseAction.Unselected || _mouseActionState == MouseAction.Rotating)
            return;

        scaledirection = Mathf.Clamp(context.ReadValue<float>(), -1f, 1f);
        //Debug.Log(scaledirection);
        if (scaledirection != 0)
        {
            //Event: OnMouseEvent
            ChangeMouseState(MouseAction.Scaling);
            ScaleObject(scaledirection);
        }
        else
        {
            //Event: OnMouseEvent
            ChangeMouseState(MouseAction.Selected);
        }
    }
    
    void RotateInitiated(InputAction.CallbackContext context)
    {
        if (!canMoveCursor)
            return;
        
        if (_mouseActionState == MouseAction.Unselected || isStillRotating)
            return;

        
        
        rotation_point = (Vector2)Input.mousePosition;
        screenMouseCoordinate = rotation_point;
        //Debug.Log(rotation_point);
    }

    void RotateReleased(InputAction.CallbackContext context)
    {
        if (!canMoveCursor)
            return;
        
        if (_mouseActionState == MouseAction.Unselected || isStillRotating)
            return;
        
        _mouseActionState = MouseAction.Rotating;
        
        //Event: OnMouseEvent
        OnMouseEvent?.Invoke(MouseAction.Rotating);
        
        
        rotation_direction = (Vector2) Input.mousePosition;
        //Calculation of which Rotation is going to initialized
        Vector2 rotation_calculated = rotation_direction - rotation_point;
        
        //Debug.Log(rotation_calculated.normalized);

        isStillRotating = true;
        //Rotate with calculated direction
        RotateObject(rotation_calculated);
        
    }
    
    void UnselectObject()
    {
        if (_mouseActionState == MouseAction.Rotating)
            return;
        
        if (!canMoveCursor)
            return;
        
        //Update Parent-Object
        UpdateObjectTransform(shadowGameObject, indicatorGameObject, true);

        //Graphics & Collision:  Shadow enabled, Indicator disabled
        ToggleActivationOfGameObject(indicatorGameObject, false);
        ToggleActivationOfGameObject(shadowGameObject, true);
        
        //Enable Rigidbody
        foregroundRigidbody.useGravity = true;
        
        //Resets Gravity
        foregroundRigidbody.isKinematic = true;
        foregroundRigidbody.isKinematic = false;
        
        //Reset Mass

        foregroundRigidbody.velocity = Vector3.zero;
        foregroundRigidbody.mass = foregroundMass;

        //Erase References
        foregroundGameObject = null;
        shadowGameObject = null;
        indicatorGameObject = null;
        foregroundRigidbody = null;

        //Event: OnMouseEvent
        ChangeMouseState(MouseAction.Unselected);
    }
    
    //Control Methods
    void MoveObject()
    {
        //Implement the Move-Method from DragObject for the Child-Object
        Vector3 v = GetMouseWorldPosition() + mouseOffset;
        
        BorderInformation b = BorderInformation.Instance;
        //!!!!!!!!!!!!!!Use Observer Pattern, when the Border moves then update it
        float grabheight = b.grabHeight.transform.position.y;
        float borderup = b.borderUp.transform.position.y;
        float borderdown = b.borderBottom.transform.position.y;
        float borderleft = b.borderLeft.transform.position.x;
        float borderright = b.borderRight.transform.position.x;
        float leftSelectedMoveBarrier = b.leftSelectedMoveBarrier.transform.position.x;
        float rightSelectedMoveBarrier = b.rightSelectedMoveBarrier.transform.position.x;
        //float borderfront = BorderInformation.Instance.borderFront.transform.position.z;
        //float borderback = BorderInformation.Instance.borderBack.transform.position.z;

        //Debug.Log("grabheight_screen = " + grabheight_screen);
        //Debug.Log("mousePos = " + mousePos);
        
        if (grabheight < borderup && v.y > grabheight)
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

        if (leftSelectedMoveBarrier > borderleft && v.x < leftSelectedMoveBarrier)
        {
            v.x = leftSelectedMoveBarrier;
        }
        else if (v.x < borderleft)
        {
            v.x = borderleft;
        }
        else if (rightSelectedMoveBarrier < borderright && v.x > rightSelectedMoveBarrier)
        {
            v.x = rightSelectedMoveBarrier;
        }
        else if (v.x > borderright)
        {
            v.x = borderright;
        }

        /*if (v.z < borderfront)
        {
            v.z = borderfront;
        }
        else if (v.z > borderback)
        {
            v.z = borderback;
        }
        */

        foregroundGameObject.transform.position = v;
        //rigidbody_selectedObjectForeground.MovePosition(v); //GetMouseWorldPosition() + mouseOffset);

    }
    
    void ScaleObject(float direction)
    {
        if (isStillRotating)
            return;
        
        //Check if Forward or Backwards
        if (direction == 0)
            return;
        
        Vector3 TransformInWorldSpace = foregroundGameObject.transform.TransformPoint(Vector3.zero);
        BorderInformation borderInformation = BorderInformation.Instance;
        
        if ((TransformInWorldSpace.z <= borderInformation.borderFront.transform.position.z && direction < 0) ||
            (TransformInWorldSpace.z >= borderInformation.borderBack.transform.position.z && direction > 0))
            return;
        

        /*Vector3 TransformInLocalSpace = selectedObjectForeground.transform.InverseTransformPoint(Vector3.forward);
        selectedObjectForeground.transform.Translate(TransformInLocalSpace * direction, Space.World);

        */

        //Reparent: Indicator
        ReparentGameObject(indicatorGameObject, emptyParentGameObject);
        
        //Move Foreground Object 
        mouseOffset += (Vector3.forward * direction * scaleSensitivity);

        
        //Move Shadow Object to remain in place
        Vector3 indicatorPosition = indicatorGameObject.transform.position;
        
        indicatorPosition -= (Vector3.forward * direction * scaleSensitivity);
        
        //For Refinements
        /*
        //Calculate Distance between LightSource and ForeGroundObjects
        Vector3 position_lightSource = new Vector3(0,0, lightsourcePosition.transform.position.z);
        Vector3 position_object = new Vector3(0, 0, foregroundGameObject.transform.position.z);

        Vector3 distance_light_obj = position_object - position_lightSource;
        */
        
        
        //Implement the Scale-Method:
        
        indicatorGameObject.transform.localScale -= (Vector3.one * direction * scaleIntensity);
        indicatorPosition -= ((Vector3.up * direction * (scaleIntensity/2)));
        
        
        //Recalculate Rigidbody Mass
       
        foregroundMass -= massIntensity * direction;

        if (foregroundMass <= 0.1)
        {
            foregroundMass = 0.1f;
        }
        
        //Reparent: Indicator
        ReparentGameObject(indicatorGameObject, foregroundGameObject);
        
        
        indicatorGameObject.transform.position = indicatorPosition;

    }
    
    void RotateObject(Vector2 direction)
    {
        
        //Check in which Direction it should rotate
        Vector3 vector3_rotation_direction = GetRotationDirection(direction);

        //If direction is 0 or unclear, then cancel method
        if (vector3_rotation_direction.Equals(Vector3.zero))
        {
            _mouseActionState = MouseAction.Selected;
            isStillRotating = false;
            return;
        }
        
        //Debug.Log("Rotating Objects");
        
        //Reparent: Indicator
        ReparentGameObject(indicatorGameObject, emptyParentGameObject);
        
        //Graphics: Indicator disabled
        //ToggleActivationOfGameObject(indicatorGameObject, false);
        
        //Rotate Child-Object
        Transform newTransform = foregroundGameObject.transform;
        Transform newTransform_indicator = indicatorGameObject.transform;
        
        newTransform.Rotate(vector3_rotation_direction,90f,Space.World);
        newTransform_indicator.Rotate(vector3_rotation_direction,90f,Space.World);
        
        UnityEngine.Quaternion quaternion = newTransform.rotation;
        UnityEngine.Quaternion quaternion_indicator = newTransform_indicator.rotation;
        
        StartCoroutine(RotateSmoothly(.15f, vector3_rotation_direction, quaternion, quaternion_indicator));
        
        
        //foregroundGameObject.transform.Rotate(vector3_rotation_direction,90f,Space.World);

        //Update Child-Child-Object
        //indicatorGameObject.transform.RotateAround(foregroundGameObject.transform.position,vector3_rotation_direction,-90f);
        //indicatorGameObject.transform.Rotate(vector3_rotation_direction,90f,Space.World);
        
        //Reparent: Indicator
        //Enable Graphics of Child-Child-Object
        //ToggleActivationOfGameObject(indicatorGameObject, true);
        
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
            edit_object.transform.localScale = target_object.transform.localScale;
    }
    
    IEnumerator RotateSmoothly(float duration, Vector3 direction, UnityEngine.Quaternion q, UnityEngine.Quaternion qi)
    {
        foregroundCollider.enabled = false;
        foregroundRigidbody.isKinematic = true;
        
        if (direction != Vector3.zero)
        {
            float t = 0.0f;
            float degree = 90 / duration;
            indicatorGameObject.transform.Rotate(direction,-90f,Space.World);
            foregroundGameObject.transform.Rotate(direction,-90f,Space.World);
            while ( t  < duration )
            {
                t += Time.deltaTime;
                foregroundGameObject.transform.Rotate(direction,degree * Time.deltaTime,Space.World);
                indicatorGameObject.transform.Rotate(direction,degree * Time.deltaTime,Space.World);
            
                yield return null;
            }
            foregroundGameObject.transform.rotation = q;
            indicatorGameObject.transform.rotation = qi;
        }

        
        ReparentGameObject(indicatorGameObject, foregroundGameObject);

        
        foregroundCollider.enabled = true;
        foregroundRigidbody.isKinematic = false;
        
        _mouseActionState = MouseAction.Selected;
        isStillRotating = false;
    }
    
    void GetSelectedObject(GameObject newSelectedObject)
    {
        foregroundGameObject = newSelectedObject;

        foregroundRigidbody = foregroundGameObject.GetComponent<Rigidbody>();
        foregroundMass = foregroundRigidbody.mass;
        foregroundRigidbody.mass = 0f;

        foregroundCollider = foregroundGameObject.transform.GetChild(0).GetChild(0).GetComponent<Collider>();
        
        //ToggleActivationOfGameObject(indicatorGameObject, true);
        
        //Get all other Objects
        shadowGameObject = foregroundGameObject.transform.GetChild(1).gameObject;
        indicatorGameObject = foregroundGameObject.transform.GetChild(2).gameObject;
        
        OnSelectedObject?.Invoke(shadowGameObject.transform.GetChild(0).gameObject);
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
        
        //Debug.Log(direction);
        //Right
        if (direction.x > 0.8f && Mathf.Abs(direction.y) < 0.7f)
        {
            result_direction = Vector3.down;
        }
        //Left
        else if (direction.x < -0.8f && Mathf.Abs(direction.y) < 0.7f)
        {
            result_direction = Vector3.up;
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
        //Debug.Log(result_direction);

        return result_direction;

    }

    float ScaleGetOffsetForShadow(float distanceFgObject, float offsetFgOGroundToOrigin, float distanceShadow) 
    {
        return ((offsetFgOGroundToOrigin*distanceShadow)/distanceFgObject);
    }

    float ScaleGetSizeFactor(float distanceFgObject, float distanceShadow)
    {
        return (distanceShadow/distanceFgObject);
    }

    void ChangeMouseState(MouseAction mA)
    {
        _mouseActionState = mA;
        OnMouseEvent?.Invoke(mA);
    }

    void ReparentGameObject(GameObject gameobject, GameObject parent)
    {
        gameobject.transform.SetParent(parent.transform);
    }

    //For Event: GameManager.OnGameStateChange
    void GameStateChanging(GameState gameState)
    {
        if (gameState != GameState.GameStart)
        {
            canMoveCursor = false;
            
            if (_mouseActionState == MouseAction.Rotating || _mouseActionState == MouseAction.Scaling)
                _mouseActionState = MouseAction.Selected;
            
            //Debug.Log("cant use cursor");
            return;
        }

        canMoveCursor = true;
        //Debug.Log("USE cursor");
    }

    void CollidingWithPlayer(bool a)
    {
        isPlayerCollidingOnSelection = a;
        Debug.Log("CollidingWithPlayer" + a);
    }
}

public enum MouseAction
{
    Selected,
    Unselected,
    Rotating,
    Scaling
}