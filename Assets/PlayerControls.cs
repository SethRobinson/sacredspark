using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControls : MonoBehaviour
{

    private CustomInput input = null;
    // Start is called before the first frame update

    public Vector2 vDir = Vector2.zero;

    static PlayerControls _this = null;

    public bool bLeftOrRightPressedThisFrame = false;
    public bool bUpOrDownThisFrame = false;
    Vector2 vDirLast = Vector2.zero;

    static public PlayerControls Get()
    {
        return _this;
    }


    public static string GetName()
    {
        return Get().name;
    }

    public CustomInput GetInput()
    {

        return input;
    }
    private void Awake()
    {
        _this = this;
        input = new CustomInput();
       
        input.Player.Movement.performed += OnMovementPerformed;
        input.Player.Movement.canceled += OnMovementCanceled;
        //input.Player.RotateLeft.started += OnRotateLeft;

        //to check if Start button was pressed from anywhere in the app:
        //if (PlayerControls.Get().GetInput().Player.Start.WasPressedThisFrame())
    }

    /*
     
       void OnRotateLeft(InputAction.CallbackContext value)
    {
      Debug.Log("Rotateleft STARTED!");
       
    }
     
     * */
    void Start()
    {
        input.Enable();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        bLeftOrRightPressedThisFrame = false;
        bUpOrDownThisFrame = false;
    }

    
    void OnMovementPerformed(InputAction.CallbackContext value)
    {

        vDir = value.ReadValue<Vector2>();
      //  Debug.Log("Got movement: X: " + vDir.x + " Y: " + vDir.y);

        if (vDirLast.x == 0)
        {

            bLeftOrRightPressedThisFrame = true;
        }

        if (vDirLast.y == 0)
        {
            bUpOrDownThisFrame = true;
        }

        vDirLast = vDir;
    }

    void OnMovementCanceled(InputAction.CallbackContext value)
    {

        vDir = value.ReadValue<Vector2>();
      //  Debug.Log("Got cancel movement: X: " + vDir.x + " Y: " + vDir.y);

        if (vDir.x == 0)
        {
            bLeftOrRightPressedThisFrame = false;
        }
        vDirLast = vDir;
    }
}
