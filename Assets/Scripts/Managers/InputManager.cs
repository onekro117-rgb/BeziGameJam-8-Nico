using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static PlayerInput PlayerInput;

    public static Vector2 Movement;
    public static bool JumpWasPressed;
    public static bool JumpIsHeld;
    public static bool JumpWasReleased;
    public static bool DashIsPressed;
    public static bool AttackWasPressed;
    public static bool MagicWasPressed;
    public static bool Magic1WasPressed;
    public static bool Magic2WasPressed;
    public static bool Magic3WasPressed;
    public static bool QTEButton1WasPressed;
    public static bool QTEButton2WasPressed;
    public static bool QTEButton3WasPressed;
    public static bool QTEButton4WasPressed;

    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _dashAction;
    private InputAction _attackAction;
    private InputAction _magicAction;
    private InputAction _magic1Action;
    private InputAction _magic2Action;
    private InputAction _magic3Action;
    private InputAction _qteButton1Action;
    private InputAction _qteButton2Action;
    private InputAction _qteButton3Action;
    private InputAction _qteButton4Action;

    private void Awake()
    {
        PlayerInput = GetComponent<PlayerInput>();

        if (PlayerInput == null)
        {
            Debug.LogWarning("InputManager: No PlayerInput component found. Disabling InputManager.");
            enabled = false;
            return;
        }
        else
        {
            _moveAction = PlayerInput.actions["Move"];
            _jumpAction = PlayerInput.actions["Jump"];
            _dashAction  = PlayerInput.actions["Dash"];
            _attackAction = PlayerInput.actions["Attack"];
            _magicAction = PlayerInput.actions["Magic"];
            
            _magic1Action = TryGetAction("Magic1");
            _magic2Action = TryGetAction("Magic2");
            _magic3Action = TryGetAction("Magic3");
            
            _qteButton1Action = PlayerInput.actions["QTEButton1"];
            _qteButton2Action = PlayerInput.actions["QTEButton2"];
            _qteButton3Action = PlayerInput.actions["QTEButton3"];
            _qteButton4Action = PlayerInput.actions["QTEButton4"];

            PlayerInput.actions.Enable();
        }
    }
    
    private InputAction TryGetAction(string actionName)
    {
        try
        {
            return PlayerInput.actions[actionName];
        }
        catch
        {
            Debug.LogWarning($"InputManager: Action '{actionName}' not found. Please update Input Actions via Tools → Update Input Actions for Magic System");
            return null;
        }
    }

    private void Update()
    {
            Movement = _moveAction.ReadValue<Vector2>();

            JumpWasPressed = _jumpAction.WasPressedThisFrame();
            JumpIsHeld = _jumpAction.IsPressed();
            JumpWasReleased = _jumpAction.WasReleasedThisFrame();

            DashIsPressed = _dashAction.IsPressed();
            
            AttackWasPressed = _attackAction.WasPressedThisFrame();
            MagicWasPressed = _magicAction.WasPressedThisFrame();
            Magic1WasPressed = _magic1Action != null && _magic1Action.WasPressedThisFrame();
            Magic2WasPressed = _magic2Action != null && _magic2Action.WasPressedThisFrame();
            Magic3WasPressed = _magic3Action != null && _magic3Action.WasPressedThisFrame();
            
            QTEButton1WasPressed = _qteButton1Action.WasPressedThisFrame();
            QTEButton2WasPressed = _qteButton2Action.WasPressedThisFrame();
            QTEButton3WasPressed = _qteButton3Action.WasPressedThisFrame();
            QTEButton4WasPressed = _qteButton4Action.WasPressedThisFrame();
    }
}
