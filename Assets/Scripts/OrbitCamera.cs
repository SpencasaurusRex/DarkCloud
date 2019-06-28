using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
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
    public float GamepadRotationSharpness = 5f;
    public float MouseRotationSharpness = 10000f;
    public float SmoothingMargin = 25f;

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
        Horizontal(rotationInput);
        Vertical(rotationInput, gamePad);
        Position();
        Rotation();

        // Find smoothed follow position
        Position();

        // Find and apply smoothed rotation
        Rotation();

        // Handle obstructions/distance
        HandleObstructions();

        // Find target position given follow position and rotation
        Vector3 targetPosition = currentFollowPosition - ((Transform.rotation * Vector3.forward) * currentDistance);

        // Offset
        targetPosition += Transform.right * Offset.x;
        targetPosition += Transform.up * Offset.y;

        // Apply transformation
        Transform.position = targetPosition;
    }

    void Horizontal(Vector3 rotationInput)
    {
        Quaternion rotationFromInput = Quaternion.Euler(FollowTransform.up * (rotationInput.x * RotationSpeed));
        PlanarDirection = rotationFromInput * PlanarDirection;
    }

    void Vertical(Vector3 rotationInput, bool gamePad)
    {
        float rotationModifierUp = 1f;
        float rotationModifierDown = 1f;
        if (gamePad)
        {
            // On a GamePad, smooth edges of vertical rotation
            float deltaMax = MaxVerticalAngle - targetVerticalAngle;
            if (deltaMax < SmoothingMargin) rotationModifierUp = deltaMax / SmoothingMargin + .1f;
            rotationModifierUp = Mathf.Clamp01(rotationModifierUp);

            float deltaMin = targetVerticalAngle - MinVerticalAngle;
            if (deltaMin < SmoothingMargin) rotationModifierDown = deltaMin / SmoothingMargin + .1f;
            rotationModifierDown = Mathf.Clamp01(rotationModifierDown);
        }

        float rotationModifier = rotationInput.y < 0 ? rotationModifierUp : rotationModifierDown;
        targetVerticalAngle -= rotationInput.y * RotationSpeed * rotationModifier;
        targetVerticalAngle = Mathf.Clamp(targetVerticalAngle, MinVerticalAngle, MaxVerticalAngle);
    }

    void Position()
    {
        currentFollowPosition = Vector3.Lerp(currentFollowPosition, FollowTransform.position,
            1 - Mathf.Exp(-FollowingSharpness * Time.deltaTime));
    }

    void Rotation()
    {
        Quaternion planarRotation = Quaternion.LookRotation(PlanarDirection, FollowTransform.up);
        Quaternion verticalRotation = Quaternion.Euler(targetVerticalAngle, 0, 0);
        Quaternion targetRotation = Quaternion.Slerp(Transform.rotation, planarRotation * verticalRotation,
            1 - Mathf.Exp(-RotationSharpness * Time.deltaTime));
        Transform.rotation = targetRotation;
    }

    void HandleObstructions()
    {
        RaycastHit closestHit = new RaycastHit {distance = Mathf.Infinity};
        obstructionCount = Physics.SphereCastNonAlloc(currentFollowPosition, ObstructionCheckRadius, -Transform.forward,
            obstructions, TargetDistance, ObstructionLayers, QueryTriggerInteraction.Ignore);

        print(obstructionCount);
        for (int i = 0; i < obstructionCount; i++)
        {
            bool ignore = IgnoredColliders.Any(t => t == obstructions[i].collider);
            if (!ignore && obstructions[i].distance < closestHit.distance && obstructions[i].distance > 0)
            {
                closestHit = obstructions[i];
            }

        }
        if (closestHit.distance < Mathf.Infinity)
        {
            distanceIsObstructed = true;
            currentDistance = Mathf.Lerp(currentDistance, closestHit.distance,
                1 - Mathf.Exp(-ObstructionSharpness * Time.deltaTime));
        }
        else
        {
            distanceIsObstructed = true;
            currentDistance = Mathf.Lerp(currentDistance, TargetDistance,
                1 - Mathf.Exp(-DistanceMovementSharpness * Time.deltaTime));
        }
    }

    public void TransitionInputMethod(InputMethod inputMethod)
    {
        if (inputMethod == InputMethod.GamePad)
        {
            RotationSharpness = GamepadRotationSharpness;
        }
        else
        {
            RotationSharpness = MouseRotationSharpness;
        }
    }
}