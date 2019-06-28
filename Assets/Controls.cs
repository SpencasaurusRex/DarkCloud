using UnityEngine;

public class Controls : MonoBehaviour
{
    public Transform Mouse;
    public Transform GamePad;

    public Vector3 MouseOffset;
    public Vector3 GamePadOffset;

    void Start()
    {
        MouseOffset = Mouse.position;
        GamePadOffset = GamePad.position;
    }

    void Update()
    {
        Mouse.position += new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), 0) * Time.deltaTime;
        GamePad.position += new Vector3(Input.GetAxis("Gamepad Camera X"), Input.GetAxis("Gamepad Camera Y"), 0) * Time.deltaTime;
    }
}
