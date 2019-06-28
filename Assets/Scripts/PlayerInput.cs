using System;
using System.Linq;
using KinematicCharacterController.Examples;
using UnityEngine;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

public class PlayerInput : MonoBehaviour
{
    public OrbitCamera Camera;
    public CharacterController Character;
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
    const string Horizontal = "X";
    const string Vertical = "Y";

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        Camera.SetFollowTransform(transform);
        Camera.IgnoredColliders = Character.GetComponentsInChildren<Collider>().ToList();
    }

    void Update()
    {
        DetermineInputMethod();
        HandleCameraInput();
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

    void HandleCameraInput()
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