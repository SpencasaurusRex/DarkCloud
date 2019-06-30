using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

public class PlayerInput : MonoBehaviour
{
    public OrbitCamera Camera;
    public CharacterController Character;
    public Transform CameraFollowPoint;

    [HideInInspector]
    public InputMethod InputMethod = InputMethod.None;

    // Specific keys/buttons
    const string GamepadAnyButton = "Gamepad Any Button";
    const string Escape = "Escape";

    // Camera controls
    const string MouseCameraX = "Mouse X";
    const string MouseCameraY = "Mouse Y";
    const string GamepadCameraX = "Gamepad Camera X";
    const string GamepadCameraY = "Gamepad Camera Y";

    // Movement controls
    const string MoveX = "X";
    const string MoveY = "Y";

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        
        Camera.SetFollowTransform(CameraFollowPoint);
        Camera.IgnoredColliders = Character.GetComponentsInChildren<Collider>().ToList();
    }

    void Update()
    {
        DetermineInputMethod();
        CameraInput();
        CharacterInput();
    }

    void DetermineInputMethod()
    {
        // Mouse click
        if (Input.GetMouseButtonDown((int) MouseButton.LeftMouse) || Input.GetMouseButtonDown((int) MouseButton.RightMouse))
        {
            TransitionInputMethod(InputMethod.KeyboardMouse);
        }
        // Escape key
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            TransitionInputMethod(InputMethod.KeyboardMouse);
        }
        // GamePad Button press
        else if (Input.GetButtonDown(GamepadAnyButton))
        {
            TransitionInputMethod(InputMethod.GamePad);
        }
    }

    void CameraInput()
    {
        if (InputMethod == InputMethod.GamePad)
        {
            float x = Input.GetAxisRaw(GamepadCameraX);
            float y = Input.GetAxisRaw(GamepadCameraY);
            Camera.ProcessInput(new Vector3(x, y, 0), true);
        }
        else
        {
            float x = Input.GetAxisRaw(MouseCameraX);
            float y = Input.GetAxisRaw(MouseCameraY);
            Camera.ProcessInput(new Vector3(x, y, 0), false);
        }
    }

    void CharacterInput()
    {
        var inputs = new CharacterController.PlayerCharacterInputs
        {
            MoveAxisForward = Input.GetAxisRaw(MoveY),
            MoveAxisRight = Input.GetAxisRaw(MoveX),
            CameraRotation = Camera.transform.rotation,
            JumpDown = false,
            CrouchDown = false,
            CrouchUp = false
        };

        Character.SetInputs(ref inputs);
    }

    void TransitionInputMethod(InputMethod inputMethod)
    {
        if (InputMethod == inputMethod) return;
        InputMethod = inputMethod;
        Camera.TransitionInputMethod(inputMethod);
    }
}

public enum InputMethod
{
    None,
    GamePad,
    KeyboardMouse
}