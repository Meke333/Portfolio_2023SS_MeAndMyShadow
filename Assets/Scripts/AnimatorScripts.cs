using System;
using UnityEngine;

public class AnimatorScripts : MonoBehaviour
{
    [SerializeField] private Animator mrShadyAnimator;
    [SerializeField] private SpriteRenderer mrShadySpriteRenderer;

    #region parameter/Animation ids

    private int id_MoveSpeed = Animator.StringToHash("MoveSpeed");
    private int id_isJumping = Animator.StringToHash("isJumping");
    private int id_isDeathExploding = Animator.StringToHash("isDeathExploding");
    private int id_isGrounded = Animator.StringToHash("isGrounded");
    private int id_isPushingObject = Animator.StringToHash("isPushingObject");
    private int id_DeathType = Animator.StringToHash("DeathType");

    private int idanim_Awake = Animator.StringToHash("Awake");
    private int idanim_Jump = Animator.StringToHash("Jump");
    private int idanim_Land = Animator.StringToHash("Land");
    private int idanim_DeathBegin = Animator.StringToHash("DeathBegin");
    private string anim_DeathBegin = "Base Layer.Death.DeathPoses.DeathBegin";

    #endregion
    
    #region parameters

    private float moveSpeed;
    private bool isJumping;
    private bool isDeathExploding;
    private bool isGrounded;
    private bool isPushingObject;
    private int deathType;
    
    #endregion

    #region Properties
    
    private float MoveSpeed
    {
        get { return moveSpeed; }
        set
        {
            if (!IsPushingObject)
            {
                if (direction < 0)
                {
                    mrShadySpriteRenderer.flipX = true;
                    //maybe localPosition must be changed
                }
                else if (direction > 0)
                {
                    mrShadySpriteRenderer.flipX = false;
                    //maybe localPosition must be changed to (0,0,0)
                }
            }
            
            moveSpeed = value;
            mrShadyAnimator.SetFloat(id_MoveSpeed, moveSpeed);
        }
    }
    private bool IsJumping
    {
        get { return isJumping; }
        set
        {
            isJumping = value;
            mrShadyAnimator.SetBool(id_isJumping, isJumping);
        }
    }
    private bool IsDeathExploading
    {
        get { return isDeathExploding; }
        set
        {
            isDeathExploding = value;
            mrShadyAnimator.SetBool(id_isDeathExploding, isDeathExploding);
        }
    }
    private bool IsGrounded
    {
        get { return isGrounded; }
        set
        {
            isGrounded = value;
            mrShadyAnimator.SetBool(id_isGrounded, isGrounded);
        }
    }
    private bool IsPushingObject
    {
        get { return isPushingObject; }
        set
        {
            isPushingObject = value;
            mrShadyAnimator.SetBool(id_isPushingObject, isPushingObject);
        }
    }
    private int DeathType
    {
        get { return deathType; }
        set
        {
            deathType = value;
            
            mrShadyAnimator.SetInteger(id_DeathType, deathType);
        }
    }
    
    #endregion

    #region variables

    private int direction;
    [SerializeField] private FigureCharacterState _figureCharacterState;

    #endregion
    
    private void Awake()
    {
        mrShadyAnimator = GetComponent<Animator>();
        mrShadySpriteRenderer = GetComponent<SpriteRenderer>();
        
        MoveSpeed = 0;
        IsJumping = false;
        IsDeathExploading = false;
        IsGrounded = false;
        IsPushingObject = false;
        DeathType = 0;

        PlayerStateChanging(FigureCharacterState.Awake);
        PlayerStateChanging(FigureCharacterState.Idle);

        mrShadyAnimator.Play(idanim_Awake);
    }

    private void OnEnable()
    {
        FigureCharacterController.OnFigureCharacterStateChanging += PlayerStateChanging;
        FigureCharacterController.OnDirectionChanging += DirectionChange;

    }

    private void OnDisable()
    {
        FigureCharacterController.OnFigureCharacterStateChanging -= PlayerStateChanging;
        FigureCharacterController.OnDirectionChanging -= DirectionChange;
    }
    

    void PlayerStateChanging(FigureCharacterState figureCharacterState)
    {
        //Debug.Log("AnimatorState Changing");
        switch (figureCharacterState)
        {
            case FigureCharacterState.Awake:
                _figureCharacterState = FigureCharacterState.Awake;
                MoveSpeed = 0;
                IsJumping = false;
                IsDeathExploading = false;
                IsGrounded = true;
                IsPushingObject = false;
                DeathType = -1;
                break;
            
            case FigureCharacterState.Idle:
                _figureCharacterState = FigureCharacterState.Idle;
                MoveSpeed = 0;
                IsJumping = false;
                IsGrounded = true;
                IsPushingObject = false;
                DeathType = 0; //On Ground
                break;
            
            case FigureCharacterState.Walk:
                _figureCharacterState = FigureCharacterState.Walk;
                MoveSpeed = 1;
                IsJumping = false;
                IsGrounded = true;
                IsPushingObject = false;
                DeathType = 0; //On Ground
                break;
            
            case FigureCharacterState.Run:
                _figureCharacterState = FigureCharacterState.Run;
                MoveSpeed = 2;
                IsJumping = false;
                IsGrounded = true;
                IsPushingObject = false;
                DeathType = 0; //On Ground
                break;
            
            case FigureCharacterState.Jump:
                _figureCharacterState = FigureCharacterState.Jump;
                IsJumping = true;
                IsGrounded = false;
                IsPushingObject = false;
                DeathType = 1; //In Air
                mrShadyAnimator.Play(idanim_Jump);
                break;
            
            case FigureCharacterState.Fall:
                _figureCharacterState = FigureCharacterState.Fall;
                IsJumping = false;
                IsGrounded = false;
                IsPushingObject = false;
                DeathType = 1; //In Air
                break;
            
            case FigureCharacterState.Push:
                _figureCharacterState = FigureCharacterState.Push;
                IsPushingObject = true;
                DeathType = 0; //OnGround
                break;
            
            case FigureCharacterState.Dead:
                _figureCharacterState = FigureCharacterState.Dead;
                mrShadyAnimator.Play(anim_DeathBegin);
                break;
        }
    }

    void DirectionChange(int dir)
    {
        //Debug.Log("DirectionChange Initiated" + dir);
        direction = dir < 0 ? -1 : dir > 0 ? 1 : 0;
        
        if (_figureCharacterState == FigureCharacterState.Idle || _figureCharacterState == FigureCharacterState.Walk || _figureCharacterState == FigureCharacterState.Run)
        {
            //Debug.Log("aaaaa");
            int absDir = Math.Abs(dir);
            
            if (absDir == 0)
            {
                _figureCharacterState = FigureCharacterState.Idle;
            }
            else if (absDir < 2)
            {
                _figureCharacterState = FigureCharacterState.Walk;
            }
            else
            {
                _figureCharacterState = FigureCharacterState.Run;
            }

            PlayerStateChanging(_figureCharacterState);
            
        }
    }

    
}



