using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct playerControls
{
    public string moveUp;
    public string moveLeft;
    public string moveRight;
    public string moveDown;
    public string moveAxis;
    public string shoot;
    public string grapple;
    public string boost;
    public string horizontalAxis;
    public string verticalAxis;
    public string aim;
    public bool axisMovement;
}

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;
    private Dictionary<string, KeyCode> keys;

    [SerializeField]
    private Player playerOne;
    [SerializeField]
    private Player playerTwo;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else if(Instance != this)
        {
            Destroy(this);
        }

        keys = new Dictionary<string, KeyCode>();

        keys.Add("W", KeyCode.W);
        keys.Add("A", KeyCode.A);
        keys.Add("S", KeyCode.S);
        keys.Add("D", KeyCode.D);
        keys.Add("Shift", KeyCode.LeftShift);
        keys.Add("LeftMouse", KeyCode.Mouse0);
        keys.Add("RightMouse", KeyCode.Mouse1);

        UpdateInputSources();

        string[] connectedControllers = Input.GetJoystickNames();

        for(int i = 0; i < connectedControllers.Length; i++)
        {
            Debug.Log("Controller " + i + ": " + connectedControllers[i]);
        }
    }

    /// <summary>
    /// Updates the players input schemes when a new controller is added
    /// </summary>
    private void UpdateInputSources()
    {
        playerOne.controls = setKeyboardControls();
        playerTwo.controls = setControllerControls(1);
    }

    /// <summary>
    /// Checks if the input was pressed this frame
    /// </summary>
    /// <param name="inputName">The input to check</param>
    /// <returns>Whether or not the input has been pressed this frame</returns>
    public bool GetInputDown(string inputName)
    {
        if (keys.ContainsKey(inputName))
        {
            return Input.GetKeyDown(keys[inputName]);
        }
        return false;
    }

    /// <summary>
    /// Checks if the input was released this frame
    /// </summary>
    /// <param name="inputName">The input to check</param>
    /// <returns>Whether or not the input has been released this frame</returns>
    public bool GetInputUp(string inputName)
    {
        if (keys.ContainsKey(inputName))
        {
            return Input.GetKeyUp(keys[inputName]);
        }
        return false;
    }

    /// <summary>
    /// Checks if the input is currently down
    /// </summary>
    /// <param name="inputName">The input to check</param>
    /// <returns>Whether or not the input is currently down</returns>
    public bool GetInput(string inputName)
    {
        if (keys.ContainsKey(inputName))
        {
            return Input.GetKey(keys[inputName]);
        }

        switch (inputName)
        {
            case "Controller1Shoot":
            case "Controller2Shoot":
            case "Controller1Boost":
            case "Controller2Boost":
            case "Controller1Grapple":
            case "Controller2Grapple":
                float value = Input.GetAxisRaw(inputName);

                if(value >= 1)
                {
                    return true;
                }
                return false;
        }

        Debug.LogWarning("Fell through InputManager GetInput " + inputName);
        return false;
    }

    /// <summary>
    /// Returns a value from -1 to 1 depending on the control state
    /// </summary>
    /// <param name="axisName">The axis to check</param>
    /// <returns>The value of the axis</returns>
    public float GetAxis(string axisName, Player player)
    {
        switch (axisName)
        {
            case "Controller1X":
            case "Controller1Y":
            case "Controller2X":
            case "Controller2Y":
            case "Controller1MoveX":
            case "Controller1MoveY":
            case "Controller2MoveX":
            case "Controller2MoveY":
                Input.GetAxis(axisName);
                break;
            case "MouseX":
            case "MouseY":
                Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 mouseDistance = mousePosition - player.Position;

                if(axisName == "MouseX")
                {
                    if(Mathf.Abs(mouseDistance.x) > 1)
                    {
                        return mouseDistance.x / Mathf.Abs(mouseDistance.x);
                    }
                    else
                    {
                        return mouseDistance.x;
                    }
                }
                else
                {
                    if (Mathf.Abs(mouseDistance.y) > 1)
                    {
                        return mouseDistance.y / Mathf.Abs(mouseDistance.y);
                    }
                    else
                    {
                        return mouseDistance.y;
                    }
                }
        }

        Debug.LogError("Fell through InputManager GetAxis switch statement.");
        return 0.0f;
    }

    /// <summary>
    /// Returns a vector2 representing the direction of two axis
    /// </summary>
    /// <param name="inputName">The controller to check input for</param>
    /// <returns>The direction representation of two axis</returns>
    public Vector2 GetDirection(string inputName, Player player)
    {
        Vector2 direction;

        switch (inputName)
        {
            case "Controller1":
                direction = new Vector2(Input.GetAxis("Controller1X"), Input.GetAxis("Controller1Y"));
                return direction.normalized;
            case "Controller2":
                direction = new Vector2(Input.GetAxis("Controller2X"), Input.GetAxis("Controller2Y"));
                return direction.normalized;
            case "Controller1Move":
                direction = new Vector2(Input.GetAxis("Controller1MoveX"), Input.GetAxis("Controller1MoveY"));
                return direction.normalized;
            case "Controller2Move":
                direction = new Vector2(Input.GetAxis("Controller2MoveX"), Input.GetAxis("Controller2MoveY"));
                return direction.normalized;
            case "Mouse":
                Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                direction = mousePosition - player.Position;
                return direction.normalized;
        }

        Debug.LogError("Fell through InputManager GetDirection switch statement.");
        return Vector2.zero;
    }

    /// <summary>
    /// Sets up a control object for keyboard and mouse input
    /// </summary>
    /// <param name="controls">The controls to set</param>
    public playerControls setKeyboardControls()
    {
        playerControls controls = new playerControls();

        controls.moveUp = "W";
        controls.moveLeft = "A";
        controls.moveRight = "D";
        controls.moveDown = "S";
        controls.moveAxis = "Mouse";
        controls.shoot = "LeftMouse";
        controls.grapple = "RightMouse";
        controls.boost = "Shift";
        controls.horizontalAxis = "MouseX";
        controls.verticalAxis = "MouseY";
        controls.aim = "Mouse";
        controls.axisMovement = false;

        return controls;
    }

    /// <summary>
    /// Sets up a control object for gamepad controls
    /// </summary>
    /// <param name="controls">The controls to set</param>
    public playerControls setControllerControls(int controller)
    {
        playerControls controls = new playerControls();
        string gamepad = "Controller" + controller;

        controls.moveUp = "none";
        controls.moveLeft = "none";
        controls.moveRight = "none";
        controls.moveDown = "none";
        controls.moveAxis = gamepad + "Move";
        controls.shoot = gamepad + "Shoot";
        controls.grapple = gamepad + "Grapple";
        controls.boost = gamepad + "Boost";
        controls.horizontalAxis = gamepad + "X";
        controls.verticalAxis = gamepad + "Y";
        controls.aim = gamepad;
        controls.axisMovement = true;

        return controls;
    }
}
