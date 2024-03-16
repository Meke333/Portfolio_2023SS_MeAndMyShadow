using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraBehavior : MonoBehaviour
{
    //Singleton Pattern
    public static CameraBehavior Instance;
    
    //Player
    [SerializeField] private GameObject player;

    //Settings
    [SerializeField] private bool shouldMoveHorizontal;
    [SerializeField] private bool shouldMoveVertical;
    [SerializeField] private bool shouldMoveCamera;
    
    
    //Borders
    [SerializeField] private Transform leftUpBorder;
    [SerializeField] private Transform rightDownBorder;
    
    //Offset
    private Vector3 offsetToPlayer;
    
    //Animator
    private Animator _cameraAnimator;
    private static readonly int Death = Animator.StringToHash("Death");

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        
        
        /*if (Camera.main != null)
            camera = Camera.main.gameObject;
        */
        
        player = GameObject.FindWithTag("Player");
        if (player == null)
            this.gameObject.SetActive(false);

        offsetToPlayer = (player.transform.position - transform.position);
        //Debug.Log("offset Camera-Player = " + offsetToPlayer);

        _cameraAnimator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        FigureCharacterController.OnMovement += ShouldCameraFollowPlayer;
        GameManager.OnPlayerDied += FocusOnPlayerDeath;
        GameManager.OnStartingWithCheckPoint += MoveStartPosition;
    }

    private void OnDisable()
    {
        FigureCharacterController.OnMovement -= ShouldCameraFollowPlayer;
        GameManager.OnPlayerDied -= FocusOnPlayerDeath;
        GameManager.OnStartingWithCheckPoint -= MoveStartPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if (!shouldMoveCamera)
            return;

        MoveCamera();
    }

    void ShouldCameraFollowPlayer(bool value)
    {
        shouldMoveCamera = value;
    }

    void MoveCamera()
    {
        Vector2 newCameraPosition = new Vector2(0,0);
        
        if (shouldMoveHorizontal)
        {
            newCameraPosition = Vector2.right * player.transform.position.x;
            if (player.transform.position.x <= leftUpBorder.position.x)
            {
                
                newCameraPosition = Vector2.right * leftUpBorder.position.x;
            }
            else if (player.transform.position.x >= rightDownBorder.position.x)
            {
                newCameraPosition = Vector2.right * rightDownBorder.position.x;
            }
        }
        
        if (shouldMoveVertical)
        {
            newCameraPosition = Vector2.up * player.transform.position.y;
            if (player.transform.position.y >= leftUpBorder.position.y)
            {
                newCameraPosition = Vector2.up * leftUpBorder.position.y;
            }
            else if (player.transform.position.y <= rightDownBorder.position.y)
            {
                newCameraPosition = Vector2.up * rightDownBorder.position.y;
            }
        }

        transform.position = new Vector3(shouldMoveHorizontal ? newCameraPosition.x - offsetToPlayer.x: transform.position.x, shouldMoveVertical ? newCameraPosition.y - offsetToPlayer.y : transform.position.y , transform.position.z);
    }

    void FocusOnPlayerDeath()
    {
        
        _cameraAnimator.SetTrigger(Death);
        
        shouldMoveCamera = false;
        Vector3 positionPlayer = player.transform.position;
        transform.position = new Vector3(positionPlayer.x, positionPlayer.y, transform.position.z);
        
    }

    void MoveStartPosition(Vector3 newPosition, int i)
    {
        float x = newPosition.x + offsetToPlayer.x;
        Vector3 a = this.transform.position;
        a.x = x;
        this.transform.position = a;
    }
}
