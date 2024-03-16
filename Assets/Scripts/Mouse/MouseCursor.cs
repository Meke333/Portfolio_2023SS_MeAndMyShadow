using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseCursor : MonoBehaviour
{
    //Singleton Pattern
    public MouseCursor Instance;
    [SerializeField] private Texture2D mouse_normal;
    [SerializeField] private Texture2D mouse_grab;
    [SerializeField] private Vector2 cursor_position;
    
    private void Awake()
    {
        //Singleton Pattern
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }

        Instance = this;
    }

    private void OnEnable()
    {
        MouseControls.OnMouseEvent += MouseAction_Change;
        Cursor.SetCursor(mouse_normal, cursor_position, CursorMode.Auto);
    }

    private void OnDisable()
    {
        MouseControls.OnMouseEvent -= MouseAction_Change;
    }

    private void MouseAction_Change(MouseAction mouseAction)
    {
        switch (mouseAction)
        {

            case MouseAction.Selected:
                Cursor.SetCursor(mouse_grab, cursor_position, CursorMode.Auto);
                break;
            case MouseAction.Unselected:
                Cursor.SetCursor(mouse_normal, cursor_position, CursorMode.Auto);
                break;
            case MouseAction.Rotating:
                break;
            case MouseAction.Scaling:
                break;
        }
    }
}
