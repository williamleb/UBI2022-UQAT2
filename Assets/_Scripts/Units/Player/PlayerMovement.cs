using System.Collections;
using Fusion;
using Interfaces;
using UnityEngine;
using Utilities;
using Utilities.Extensions;
using VFX;

namespace Units.Player
{
    public partial class PlayerEntity : IVelocityObject
    {
        [Header("Movement")] 
        [SerializeField] private DustTrailController dustTrailController;
        [SerializeField] private Transform cameraPoint;
        private NetworkRigidbody nRb;
        [Networked] private NetworkBool CanMove { get; set; } = true;
        [Networked] private Vector3 MoveDirection { get; set; } = Vector3.zero;

        public Vector3 Velocity => nRb.Rigidbody.velocity;
        public float WalkMaxSpeed => data.MoveMaximumSpeed;
        public float SprintMaxSpeed => data.SprintMaximumSpeed;
        [Networked (OnChanged = nameof(OnCurrentSpeedChanged)), Accuracy(0.5f)] public float CurrentSpeed { get; private set; }
        private static void OnCurrentSpeedChanged(Changed<PlayerEntity> changed) => changed.Behaviour.UpdateMoveAnim();
        
        
        private Vector3 cameraPointOffset;
        private float currentMaxMoveSpeed;
        private Vector3 lastMoveDirection = Vector3.zero;
        
        private TickTimer isOnWallTimer;
        private bool IsOnWall => !isOnWallTimer.ExpiredOrNotRunning(Runner);
        private bool HasMoveInput => MoveDirection.sqrMagnitude > 0.05;
        private bool IsMovingFast => CurrentSpeed >= data.SprintFumbleThreshold * data.SprintMaximumSpeed;
        private bool CanRotate => IsDashing || isRagdoll || lastMoveDirection == Vector3.zero;
        private float SpeedOnMaxSpeed => (1 + CurrentSpeed) / data.MoveMaximumSpeed;

        private void MovementAwake() => nRb = GetComponent<NetworkRigidbody>();

        private void MoveUpdate()
        {
            HandleMoveInput();
            CalculateVelocity();
            MovePlayer();
            RotatePlayer();
            dustTrailController.UpdateDustTrail(CurrentSpeed / data.SprintMaximumSpeed, isRagdoll);
            UpdateCameraPointPosition();
        }

        private void UpdateCameraPointPosition()
        {
            float targetX = lastMoveDirection.x * data.CameraPointOffset.x;
            float targetZ = lastMoveDirection.z * data.CameraPointOffset.z;
            Vector3 target = new Vector3(targetX, 0, targetZ);
            cameraPointOffset =
                Vector3.MoveTowards(cameraPointOffset, target, data.CameraPointSpeed * Runner.DeltaTime);
            cameraPoint.position = transform.position + cameraPointOffset;
        }

        private void HandleMoveInput()
        {
            MoveDirection = CanMove ? Inputs.Move.V2ToFlatV3() : Vector3.zero;

            if (!IsDashing && !InMenu && !InCustomization)
            {
                if (HasMoveInput) lastMoveDirection = MoveDirection.normalized;
                //I don't want to use normalize since I want the magnitude to be smaller than 1 sometimes 
                MoveDirection = Vector3.ClampMagnitude(MoveDirection, 1);
                ChangeMoveSpeed(Inputs.IsSprint);
            }

            if (InMenu || InCustomization)
            {
                MoveDirection = Vector3.zero;
                ChangeMoveSpeed(false);
            }
        }

        private void ChangeMoveSpeed(bool isSprinting)
        {
            if (HasMoveInput)
            {
                if (!IsAiming)
                {
                    bool canSprint = isSprinting && !inventory.HasHomework && CurrentSpeed >= data.MoveMaximumSpeed && !IsOnWall;
                    float maxMoveSpeed = canSprint ? data.SprintMaximumSpeed : data.MoveMaximumSpeed;
                    float sprintAcceleration =
                        (canSprint ? data.SprintAcceleration : data.SprintBraking) * Runner.DeltaTime;
                    currentMaxMoveSpeed = Mathf.MoveTowards(currentMaxMoveSpeed, maxMoveSpeed, sprintAcceleration);
                }
                else
                {
                    currentMaxMoveSpeed = Mathf.MoveTowards(currentMaxMoveSpeed, data.AimingMovementSpeed,
                        data.AimingMovementDeceleration);
                }
            }
            else
            {
                currentMaxMoveSpeed = Mathf.MoveTowards(currentMaxMoveSpeed, data.MoveMaximumSpeed, data.SprintBraking);
            }
        }

        private void CalculateVelocity()
        {
            if (CanMove && HasMoveInput && !IsDashing)
            {
                CurrentSpeed += data.MoveAcceleration * Runner.DeltaTime;
                CurrentSpeed = Mathf.Min(CurrentSpeed, currentMaxMoveSpeed * MoveDirection.magnitude);
            }
            else
            {
                CurrentSpeed = Mathf.MoveTowards(CurrentSpeed, 0, data.MoveDeceleration * Runner.DeltaTime);
            }
        }

        private void MovePlayer()
        {
            if (isRagdoll) return;
            Vector3 vel = nRb.Rigidbody.rotation * Vector3.forward * CurrentSpeed;
            vel.y = nRb.Rigidbody.velocity.y;
            nRb.Rigidbody.velocity = vel;
        }
        
        private void RotatePlayer()
        {
            if (CanRotate) return;
            float turnRateDivider = Mathf.Max(1, CurrentSpeed - data.MoveMaximumSpeed);
            float turnRate = data.TurnRotationSpeed * Runner.DeltaTime / turnRateDivider;
            transform.forward = CurrentSpeed < data.MoveMaximumSpeed / 2f
                ? lastMoveDirection
                : Vector3.RotateTowards(transform.forward, lastMoveDirection, turnRate, 0);
        }

        private IEnumerator AfterGetUp(bool isGettingUpBackDown)
        {
            yield return Helpers.GetWait(isGettingUpBackDown ? 0.6f : 0.533f);
            CanMove = true;
            immunityTimer = TickTimer.CreateFromSeconds(Runner, data.ImmunityTime);
            ResetGetUp();
        }

        private void ResetVelocity()
        {
            CurrentSpeed = 0;
            currentMaxMoveSpeed = data.MoveMaximumSpeed;
        }

        private void SetImmunity(bool immune)
        {
            ResetVelocity();
            nRb.Rigidbody.isKinematic = immune;
            nRb.Rigidbody.detectCollisions = !immune;
            isImmune = immune;
            CanMove = !immune;
        }
    }
}