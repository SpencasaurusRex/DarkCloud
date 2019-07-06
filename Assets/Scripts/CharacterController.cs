using System;
using System.Collections.Generic;
using KinematicCharacterController;
using UnityEditor;
using UnityEngine;

public class CharacterController : MonoBehaviour, ICharacterController
{
    public KinematicCharacterMotor Motor;
    public Animator Animator;

    [Header("Grounded")]
    public float MoveSpeed = 3f;
    public float MovementSharpness = 5;
    public float RotationSharpness = 5f;

    [Header("In Air")]
    public float AirSpeed = 1.5f;
    public float Drag = 0.1f;
    public Vector3 Gravity = new Vector3(0, -15f, 0);

    [Header("Sliding")]
    public float SlideLeftRightSpeed = 1f;
    [Tooltip("This will be multiplied by sin of sliding angle")]
    public float SlideDownSpeed = 2f;
    public Vector2 SlideDownMultiplier = new Vector2(.8f, 1.5f);
    public float MaxSlideAngle = 70;
    public float SlideSharpness = 3f;

    [Header("Other")]
    public float AnimationSharpness = 1f;
    float speedPercent;

    // Debug
    List<Tuple<Vector3, Vector3, Color>> DebugDrawVectors = new List<Tuple<Vector3, Vector3, Color>>();

    public struct PlayerCharacterInputs
    {
        public float MoveAxisForward;
        public float MoveAxisRight;
        public Quaternion CameraRotation;
        public bool JumpDown;
        public bool CrouchDown;
        public bool CrouchUp;
    }

    Vector3 moveInput;

    void Start()
    {
        Motor.CharacterController = this;
    }

    public void SetInputs(ref PlayerCharacterInputs inputs)
    {
        Vector3 clampedInputMovement = Vector3.ClampMagnitude(new Vector3(inputs.MoveAxisRight, 0, inputs.MoveAxisForward), 1f);
        Vector3 cameraDirection = Vector3.ProjectOnPlane(inputs.CameraRotation * Vector3.forward, Motor.CharacterUp).normalized;
        Quaternion cameraRotation = Quaternion.LookRotation(cameraDirection, Motor.CharacterUp);

        moveInput = cameraRotation * clampedInputMovement;
    }

    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        if (Mathf.Approximately(Motor.BaseVelocity.sqrMagnitude, 0)) return;

        var velocityDirection = Vector3.ProjectOnPlane(Motor.BaseVelocity, Motor.CharacterUp);
        if (Mathf.Approximately(velocityDirection.sqrMagnitude, 0)) return;

        Quaternion targetRotation = Quaternion.LookRotation(velocityDirection, Motor.CharacterUp);
        currentRotation =
            Quaternion.Slerp(currentRotation, targetRotation, 1f - Mathf.Exp(-RotationSharpness * deltaTime));
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {

        // Running animation
        //speedPercent = Mathf.Lerp(speedPercent, moveInput.magnitude * .8f, Time.deltaTime * AnimationSharpness);

        //if (Animator)
        //{
        //    Animator.SetFloat("speedPercent", speedPercent);
        //}

        Vector3 targetVelocity = Vector3.zero;
        if (Motor.GroundingStatus.IsStableOnGround) // TODO: Or if unstable sliding vector is less than 55 degrees to prevent slipping off edges
        {
            currentVelocity.x = currentVelocity.z = 0;
            currentVelocity += moveInput * MoveSpeed;
            // Reorient velocity on new slopes
            currentVelocity = Motor.GetDirectionTangentToSurface(currentVelocity, Motor.GroundingStatus.GroundNormal) * currentVelocity.magnitude;

            Vector3 rightInput = Vector3.Cross(moveInput, Motor.CharacterUp);
            Vector3 reorientedInput = Vector3.Cross(Motor.GroundingStatus.GroundNormal, rightInput).normalized * moveInput.magnitude;
            targetVelocity = reorientedInput * MoveSpeed;

            currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, 1 - Mathf.Exp(-MovementSharpness * deltaTime));
        }
        else
        {
            // Cancel vertical movement
            if (currentVelocity.y > 0)
            {
                currentVelocity.y = 0;
            }

            // If we're sliding against unstable ground
            if (Motor.GroundingStatus.FoundAnyGround)
            {
                UnstableGround(ref currentVelocity, deltaTime);
            }
            else
            {
                Fall(ref currentVelocity, deltaTime);
            }
        }
    }

    void UnstableGround(ref Vector3 currentVelocity, float deltaTime)
    {
        Vector3 slidingVector = Vector3.Cross(Motor.GroundingStatus.GroundNormal, Vector3.Cross(Motor.GroundingStatus.GroundNormal, Motor.CharacterUp)).normalized;
        //if (Math.Asin(-slidingVector.y) > MaxSlideAngle)
        //{
        //    // TODO: Fall
        //    return;
        //}

        Vector3 slidingVectorRight = Vector3.Cross(slidingVector, Motor.CharacterUp).normalized;
        Vector3 tangentInput = Vector3.Cross(Motor.GroundingStatus.GroundNormal, Vector3.Cross(moveInput, Motor.CharacterUp)).normalized * moveInput.magnitude;

        float slideDownModifier = Vector3.Dot(slidingVector, tangentInput);
        if (slideDownModifier > 0) slideDownModifier = 1 + slideDownModifier * SlideDownMultiplier.y;
        else slideDownModifier = 1 + slideDownModifier * SlideDownMultiplier.x;

        float slideSideModifier = SlideLeftRightSpeed * Vector3.Dot(slidingVectorRight, tangentInput);
        Vector3 targetVelocity = slidingVector * SlideDownSpeed * slideDownModifier + slidingVectorRight * slideSideModifier * -slidingVector.y;

        currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, 1 - Mathf.Exp(-SlideSharpness * deltaTime));

        DebugDraw(Motor.GroundingStatus.GroundPoint, targetVelocity, Color.black);
        DebugDraw(Motor.GroundingStatus.GroundPoint, currentVelocity, Color.white);

        //print($"GN:"+Motor.GroundingStatus.GroundNormal+"IN:"+moveInput+"SLR:"+slidingVectorRight+"TIN:"+tangentInput+"DOT:"+Vector3.Dot(slidingVectorRight, tangentInput));
        //print(Vector3.Dot(slidingVector, tangentInput) + " " + Vector3.Dot(slidingVectorRight, tangentInput));
    }

    void Fall(ref Vector3 currentVelocity, float deltaTime)
    {
        currentVelocity += Gravity * deltaTime;
        currentVelocity.x *= 1f / (1f + Drag * deltaTime);
        currentVelocity.z *= 1f / (1f + Drag * deltaTime);
    }

    public void BeforeCharacterUpdate(float deltaTime)
    {

    }

    public void PostGroundingUpdate(float deltaTime)
    {

    }

    public void AfterCharacterUpdate(float deltaTime)
    {

    }

    public bool IsColliderValidForCollisions(Collider coll)
    {
        return true;
    }

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
        
    }

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
        ref HitStabilityReport hitStabilityReport)
    {
        
    }

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition,
        Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {
        
    }

    public void OnDiscreteCollisionDetected(Collider hitCollider)
    {
        
    }

    #region Debug draw
    void DebugDraw(Vector3 origin, Vector3 offset, Color c)
    {
        DebugDrawVectors.Add(new Tuple<Vector3, Vector3, Color>(origin, offset, c));
    }

    void OnDrawGizmos()
    {
        foreach (var vectorSet in DebugDrawVectors)
        {
            Gizmos.color = vectorSet.Item3;
            Gizmos.DrawLine(vectorSet.Item1, vectorSet.Item1 + vectorSet.Item2);
        }

        DebugDrawVectors.Clear();
    }
    #endregion
}
