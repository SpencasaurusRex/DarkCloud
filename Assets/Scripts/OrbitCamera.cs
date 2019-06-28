using System;
using System.Collections.Generic;
using UnityEngine;

public class OrbitCamera : MonoBehaviour
{
    [Header("Framing")] public Camera Camera;
    public Vector2 Offset = Vector2.zero;
    public float FollowingSharpness;

    [Header("Distance")] public float DefaultDistance = 6f;
    public float MinDistance = 5f;
    public float MaxDistance = 10f;
    public float DistanceMovementSpeed = 5f;
    public float DistanceMovementSharpness = 10f;

    [Header("Rotation")] public bool InvertX = false;
    public bool InvertY = false;
    [Range(-45f, 70f)] public float DefaultVerticalAngle = 20f;
    [Range(-45f, 70f)] public float MinVerticalAngle = -45f;
    [Range(-45f, 70f)] public float MaxVerticalAngle = 70f;
    public float RotationSpeed = 1f;
    public float RotationSharpness = 10000f;

    [Header("Obstruction")] public float ObstructionCheckRadius = 0.2f;
    public LayerMask ObstructionLayers = -1;
    public float ObstructionSharpness = 10000f;
    public List<Collider> IgnoredColliders = new List<Collider>();

    public Transform Transform { get; private set; }
    public Vector3 PlanarDirection { get; private set; }
    public Transform FollowTransform { get; private set; }
    public float TargetDistance { get; set; }

    bool distanceIsObstructed;
    float currentDistance;
    float targetVerticalAngle;
    RaycastHit obstructionHit;
    int obstructionCount;
    RaycastHit[] obstructions = new RaycastHit[MaxObstructions];
    float obstructionTime;
    Vector3 currentFollowPosition;

    const int MaxObstructions = 32;

    void OnValidate()
    {
        DefaultDistance = Mathf.Clamp(DefaultDistance, MinDistance, MaxDistance);
        DefaultVerticalAngle = Mathf.Clamp(DefaultVerticalAngle, MinVerticalAngle, MaxVerticalAngle);
    }

    void Awake()
    {
        Transform = transform;

        currentDistance = TargetDistance = DefaultDistance;
        targetVerticalAngle = 0f;

        PlanarDirection = Vector3.forward;
    }

    public void SetFollowTransform(Transform t)
    {
        FollowTransform = t;
        PlanarDirection = FollowTransform.forward;
        currentFollowPosition = FollowTransform.position;
    }

    public void ProcessInput(Vector3 rotationInput, bool gamePad)
    {
        if (!FollowTransform) return;
        if (InvertX) rotationInput.x *= -1;
        if (InvertY) rotationInput.y *= -1;

        // Process rotation
        // Horizontal
        Quaternion rotationFromInput = Quaternion.Euler(FollowTransform.up * (rotationInput.x * RotationSpeed));
        PlanarDirection = rotationFromInput * PlanarDirection;
        // Vertical
        float rotationModifier = 1f;
        if (gamePad)
        {
            // On a GamePad, smooth edges of vertical rotation
            if (MaxVerticalAngle - targetVerticalAngle < 20) rotationModifier = .5f;
            if (MinVerticalAngle - targetVerticalAngle < -20) rotationModifier = .5f;
        }
        targetVerticalAngle -= rotationInput.y * RotationSpeed * rotationModifier;
        targetVerticalAngle = Mathf.Clamp(targetVerticalAngle, MinVerticalAngle, MaxVerticalAngle);

        // Find smoothed follow position
        currentFollowPosition = Vector3.Lerp(currentFollowPosition, FollowTransform.position,
            1 - Mathf.Exp(-FollowingSharpness * Time.deltaTime));

        // Find smoothed rotation
        Quaternion planarRotation = Quaternion.LookRotation(PlanarDirection, FollowTransform.up);
        Quaternion verticalRotation = Quaternion.Euler(targetVerticalAngle, 0, 0);
        Quaternion targetRotation = Quaternion.Slerp(Transform.rotation, planarRotation * verticalRotation,
            1 - Mathf.Exp(-RotationSharpness * Time.deltaTime));

        // Apply rotation
        Transform.rotation = targetRotation;

        // Find target position given follow position and rotation
        Vector3 targetPosition = currentFollowPosition - ((targetRotation * Vector3.forward) * currentDistance);

        // Offset
        targetPosition += Transform.right * Offset.x;
        targetPosition += Transform.up * Offset.y;

        // Apply transformation
        Transform.position = targetPosition;
    }
}