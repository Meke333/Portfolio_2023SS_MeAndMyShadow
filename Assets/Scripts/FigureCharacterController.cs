using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class FigureCharacterController : MonoBehaviour
{
    #region adjustable
    
    private Rigidbody _rigidbody;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpForce;
    
    [SerializeField] private LayerMask layerToCheck;

    #endregion
    
    #region Jump/Walk variables

    private float currentAbsSpeed;
    

    //Variables for HoldJump
    [SerializeField] float jumpStartTime;
    [SerializeField] private float jumpTime;
    [SerializeField] private bool isJumping;
    [SerializeField] private float jumpInterval;
    [SerializeField] private float jumpStartInterval = 0.05f;
    private float airBorneTime;

    private float groundtriggerIgnoreTimer;
    private float groundTriggerIgnoreTimer_maxValue = 0.1f;

    [SerializeField] private float kojoteTimeReplika;
    
    
    [SerializeField] private bool isNotGrounded;

    private bool IsNotGrounded
    {
        get {return isNotGrounded;}
        set
        {
            if (!isPlayerDead)
                OnMovement?.Invoke(true);
            
            if (isNotGrounded == value)
                return;
            
            isNotGrounded = value;
        }
    }
    
    [SerializeField] private bool isAbleToPush;

    #endregion

    #region Input Variables

    [SerializeField] private bool canMove;
    private bool isPlayerDead;

    //InputSystem
    private PlayerInput input;
    
    private Vector2 moveDirection;
    private bool input_isMoving;
    private bool input_isJumping;


    #endregion

    #region Events

    public static event Action<bool> OnMovement;
    public static event Action<float> OnLanding;
    
    //animator
    public static event Action<FigureCharacterState> OnFigureCharacterStateChanging;
    public static event Action<int> OnDirectionChanging;
    
    //public static event Action<DeathTypeState> OnPlayerDeathChange;
    
    //sound
    public static event Action<PlayerSoundState> OnPlayerSFXPlaying;

    #endregion
    
    private void Awake()
    {
        input = new PlayerInput();
        
        input.ShadowControls.Enable();

        input.ShadowControls.Shadow_Movement.started += MovePressed;
        input.ShadowControls.Shadow_Movement.canceled += MovePressed;
        input.ShadowControls.Shadow_Jump.performed += JumpPressed;
        input.ShadowControls.Shadow_Jump.canceled += JumpPressed;
        
        //Scaling the Jump & Movespeeed 
        moveSpeed *= transform.localScale.x;
        jumpForce *= transform.localScale.x;

    }

    private void OnEnable()
    {
        GameManager.OnGameStateChange += GameStateChanging;
        GameManager.OnPlayerDied += PlayerDying;
        GameManager.OnStartingWithCheckPoint += MoveStartPosition;
        PlayerTriggerEvent.OnPushStateChange += ChangePlayerPushState;
    }

    private void OnDisable()
    {
        input.ShadowControls.Disable();
        
        GameManager.OnGameStateChange -= GameStateChanging;
        GameManager.OnPlayerDied -= PlayerDying;
        GameManager.OnStartingWithCheckPoint -= MoveStartPosition;
        PlayerTriggerEvent.OnPushStateChange -= ChangePlayerPushState;
        
        input.ShadowControls.Shadow_Movement.started -= MovePressed;
        input.ShadowControls.Shadow_Movement.canceled -= MovePressed;
        input.ShadowControls.Shadow_Jump.performed -= JumpPressed;
        input.ShadowControls.Shadow_Jump.canceled -= JumpPressed;
    }

    void Start()
    {
        _rigidbody = this.GetComponent<Rigidbody>();
        IsNotGrounded = false;
        
        OnPlayerSFXPlaying?.Invoke(PlayerSoundState.Spawn);
    }
    
    void Update()
    {
        jumpInterval -= jumpInterval < 0 ? 0 : Time.deltaTime;
        groundtriggerIgnoreTimer -= groundtriggerIgnoreTimer < 0 ? 0 : Time.deltaTime;
        airBorneTime += isNotGrounded ? Time.deltaTime : 0;
    }

    private void FixedUpdate()
    {
        if (!canMove)
            return;
        
        if (input_isJumping)
            Jump();
        
        if (input_isMoving)
            Move();
    }

    //Collision with Ground
    private void OnTriggerEnter(Collider other)
    {
        
        if (groundtriggerIgnoreTimer <= 0)
        {
            CheckIfPlayerIsOnGround(other);
            airBorneTime = 0;
        }
    }

    //Here to prevent isNotGrounded will be checked forever
    private void OnTriggerStay(Collider other)
    {
        if (groundtriggerIgnoreTimer <= 0)
        {
            CheckIfPlayerIsOnGround(other);
        }
    }
    

    
    //If you leave the ground, you are not on the ground
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("CheckPoint"))
            return;

        Invoke(nameof(UngroundedDelay), kojoteTimeReplika);
        groundtriggerIgnoreTimer = groundTriggerIgnoreTimer_maxValue;

        if (!isJumping)
        {
            OnFigureCharacterStateChanging?.Invoke(FigureCharacterState.Fall);
        }
    }

    //Input System Methods
    void MovePressed(InputAction.CallbackContext context)
    {
        if (isPlayerDead)
            return;
        
        moveDirection = context.ReadValue<Vector2>();
        //Debug.Log(moveDirection);
        input_isMoving = moveDirection.x != 0 || moveDirection.y != 0;

        OnMovement?.Invoke(input_isMoving);
        
        //Debug.Log("Change MoveDirection");
        OnDirectionChanging?.Invoke((int) moveDirection.x);
        
        if (!isJumping && !IsNotGrounded && !input_isJumping)
            ChangeGroundedPlayerState();

    }

    void JumpPressed(InputAction.CallbackContext context)
    {
        if (isPlayerDead)
            return;
        
        input_isJumping = context.ReadValueAsButton();

        if (!input_isJumping) //Button released
        {

            if (IsNotGrounded)
                OnFigureCharacterStateChanging?.Invoke(FigureCharacterState.Fall);
            
            isJumping = false;
        }
            
        else
        {
            if (!IsNotGrounded)
            {
                
                OnFigureCharacterStateChanging?.Invoke(FigureCharacterState.Jump);
                OnPlayerSFXPlaying?.Invoke(PlayerSoundState.Jump);
                groundtriggerIgnoreTimer = .5f;
            }
            //Debug.Log("Jump");
            
               
            
            Invoke(nameof(UnJumpDelay), .5f);
        }

    }
    
    //Player Movement methods
    void Move()
    {
        _rigidbody.MovePosition(new Vector3 (transform.position.x + (moveDirection.x * moveSpeed * Time.fixedDeltaTime), transform.position.y, transform.position.z) );
        //_rigidbody.AddForce(new Vector3 ( (moveHorizontal * moveSpeed), 0, 0), ForceMode.Impulse); Accellerates to much...
        
    }

    void Jump()
    {
        if (input_isJumping && !IsNotGrounded && !isJumping)
        {
            IsNotGrounded = true;
            isJumping = true;
            jumpTime = jumpStartTime;
            _rigidbody.velocity = Vector3.zero;
            

            jumpInterval = jumpStartInterval;
            
            
            OnFigureCharacterStateChanging?.Invoke(FigureCharacterState.Jump);
            
        }

        if ((input_isJumping && isJumping))
        {
            if (jumpTime > 0)
            {
                _rigidbody.AddForce(new Vector3(0, jumpForce, 0f), ForceMode.Impulse);
                jumpTime -= Time.deltaTime;
            }
            else
            {
                _rigidbody.AddForce(new Vector3(0, -jumpForce*2 , 0f), ForceMode.Impulse);
                input_isJumping = false;
                isJumping = false;
            }
        }
        
    }

    void CheckIfPlayerIsOnGround(Collider other)
    {
        if (!IsNotGrounded || isPlayerDead)
            return;
        
        if ((layerToCheck.value & (1 << other.gameObject.layer)) != 0)
        {
            Debug.Log("Landed On Ground");
            if (canMove)
                OnLanding?.Invoke(airBorneTime);

            IsNotGrounded = false;
            if (!isPlayerDead && !isJumping)
                OnPlayerSFXPlaying?.Invoke(PlayerSoundState.Land);
            
            if (input_isJumping)
                OnFigureCharacterStateChanging?.Invoke(FigureCharacterState.Jump);
            else
                OnFigureCharacterStateChanging?.Invoke(FigureCharacterState.Idle);
            ChangeGroundedPlayerState();
            
            
        }
    }
    void UngroundedDelay()
    {
        IsNotGrounded = true;
        
        
        /*if (isJumping)
            OnFigureCharacterStateChanging?.Invoke(FigureCharacterState.Fall);
        */   
    }

    void UnJumpDelay()
    {
        if (IsNotGrounded)
            OnFigureCharacterStateChanging?.Invoke(FigureCharacterState.Fall);
    }

    //Events
    void GameStateChanging(GameState gamestate)
    {
        if (gamestate != GameState.GameStart)
        {
            canMove = false;
            return;
        }
        canMove = true;

    }

    void PlayerDying()
    {
        isPlayerDead = true;
        OnPlayerSFXPlaying?.Invoke(PlayerSoundState.Dead);
        OnFigureCharacterStateChanging?.Invoke(FigureCharacterState.Dead);
        canMove = false;
        Destroy(gameObject.GetComponent<CapsuleCollider>());
        _rigidbody.isKinematic = false;
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.useGravity = false;
        Debug.Log("PlayerDead Invoked");
    }

    void ChangePlayerPushState(bool nearBox)
    {
        isAbleToPush = nearBox;

        if (IsNotGrounded)
            return;
        
        ChangeGroundedPlayerState();
        

    }

    void ChangeGroundedPlayerState()
    {
        if (IsNotGrounded)
            return;
        
        currentAbsSpeed = Math.Abs(moveDirection.x);
        if (currentAbsSpeed == 0)
            OnFigureCharacterStateChanging?.Invoke(FigureCharacterState.Idle);
        
        else
        {
            if (isAbleToPush)
                OnFigureCharacterStateChanging?.Invoke(FigureCharacterState.Push);
            
            else if (currentAbsSpeed < 2)
                OnFigureCharacterStateChanging?.Invoke(FigureCharacterState.Walk);
            
            else
                OnFigureCharacterStateChanging?.Invoke(FigureCharacterState.Run);
        }
    }

    void MoveStartPosition(Vector3 newPosition, int i)
    {
        float z = this.transform.position.z;
        Vector3 a = newPosition;
        a.z = z;
        this.transform.position = a;
    }

}

public enum FigureCharacterState
{
    Awake,
    Idle,   //)
    Walk,   // > Grounded
    Run,    //)
    Jump,   
    Fall,
    Push,
    Dead,
}

/*
public enum DeathTypeState
{
    
} 
*/


