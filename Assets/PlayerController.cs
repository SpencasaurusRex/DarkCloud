using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Editor
    public Transform CamPivot;
    public Transform Cam;
    public float Speed;
    public float SmoothTime;

    // Camera
    float heading;
    Vector3 camF;
    Vector3 camR;

    // Input
    Vector2 input;
    Vector3 intent;

    // Physics
    Vector3 velocity;
    Vector3 smoothVelocity;

    CharacterController controller;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        HandleInput();
        CalculateCamera();
        Move();
    }

    void HandleInput()
    {
        heading += Input.GetAxis("Mouse X") * Time.deltaTime * 180;
        CamPivot.rotation = Quaternion.Euler(0, heading, 0);

        input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        input = Vector2.ClampMagnitude(input, 1);
    }

    void CalculateCamera()
    {
        camF = Cam.forward;
        camR = Cam.right;
        camF.y = 0;
        camR.y = 0;
        camF.Normalize();
        camR.Normalize();
    }

    void Move()
    {
        intent = (camF * input.y + camR * input.x) * Speed;
        velocity = Vector3.SmoothDamp(velocity, intent, ref smoothVelocity, SmoothTime);

        controller.Move(velocity * Time.deltaTime);
    }
}
