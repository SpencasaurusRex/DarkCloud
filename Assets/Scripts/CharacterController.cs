using KinematicCharacterController;
using UnityEngine;

public class CharacterController : MonoBehaviour, ICharacterController
{
    public KinematicCharacterMotor Motor;
    public Animator Animator;

    public float MoveSpeed = 3;
    public Vector3 Gravity = new Vector3(0, -15f, 0);

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

    void Update()
    {
        
    }

    public void SetInputs(ref PlayerCharacterInputs inputs)
    {
        Vector3 clampedInputMovement = Vector3.ClampMagnitude(new Vector3(inputs.MoveAxisRight, 0, inputs.MoveAxisForward),1f);
        Vector3 cameraDirection = Vector3.ProjectOnPlane(inputs.CameraRotation * Vector3.forward, Motor.CharacterUp).normalized;
        Quaternion cameraRotation = Quaternion.LookRotation(cameraDirection, Motor.CharacterUp);

        moveInput = cameraRotation * clampedInputMovement;
    }

    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        // Face towards movement TODO smoothing
        if (Mathf.Approximately(Motor.BaseVelocity.sqrMagnitude, 0)) return;

        var velocityDirection = Vector3.ProjectOnPlane(Motor.BaseVelocity, Motor.CharacterUp);
        if (Mathf.Approximately(velocityDirection.sqrMagnitude, 0)) return;

        currentRotation = Quaternion.LookRotation(velocityDirection, Motor.CharacterUp);
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        currentVelocity.x = currentVelocity.z = 0;
        currentVelocity += moveInput * MoveSpeed;
        //if (!Motor.GroundingStatus.IsStableOnGround)
        {
            currentVelocity += Gravity * Time.deltaTime;
        }
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
}
