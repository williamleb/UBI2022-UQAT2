using System;
using System.Collections;
using Fusion;
using Systems.Network;
using UnityEngine;
using Utilities.Extensions;
using VFX;
using Random = UnityEngine.Random;

namespace Units.Player
{
    public partial class PlayerEntity
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
        public float CurrentSpeed => velocity;

        private Vector3 cameraPointOffset;
        private float currentMaxMoveSpeed;
        private float velocity;
        private Vector3 lastMoveDirection = Vector3.zero;
        private bool isTurningAround;
        private Coroutine rotateCoroutine;
        private bool HasMoveInput => MoveDirection.sqrMagnitude > 0.05;
        private bool IsMovingFast => velocity >= data.SprintFumbleThreshold * data.SprintMaximumSpeed;

        private void MovementAwake()
        {
            nRb = GetComponent<NetworkRigidbody>();
        }

        private void MoveUpdate(NetworkInputData inputData)
        {
            HandleMoveInput(inputData);
            CalculateVelocity();
            MovePlayer();
            dustTrailController.UpdateDustTrail(velocity/data.SprintMaximumSpeed, isRagdoll);
            UpdateCameraPointPosition();
        }

        private void UpdateCameraPointPosition()
        {
            float targetX = lastMoveDirection.x * data.CameraPointOffset.x;
            float targetZ = lastMoveDirection.z * data.CameraPointOffset.z;
            Vector3 target = new Vector3(targetX, 0, targetZ);
            cameraPointOffset = Vector3.MoveTowards(cameraPointOffset, target, data.CameraPointSpeed * Runner.DeltaTime);
            cameraPoint.position = transform.position + cameraPointOffset;
        }

        private void HandleMoveInput(NetworkInputData inputData)
        {
            MoveDirection = CanMove ? inputData.Move.V2ToFlatV3() : Vector3.zero;
            
            if (!IsDashing && !inMenu && !InCustomization)
            {
                if (HasMoveInput)
                {
                    if (lastMoveDirection != MoveDirection)
                    {
                        if (rotateCoroutine != null) StopCoroutine(rotateCoroutine);
                        lastMoveDirection = MoveDirection;
                        rotateCoroutine = StartCoroutine(RotatePlayerTowards());
                    }
                }

                float directionDot = Vector3.Dot(transform.forward, lastMoveDirection);
                if (directionDot <= -0.2 && velocity == 0) isTurningAround = true;
                if (directionDot >= 0.99) isTurningAround = false;
                //I don't want to use normalize since I want the magnitude to be smaller than 1 sometimes 
                MoveDirection = Vector3.ClampMagnitude(MoveDirection, 1);
                ChangeMoveSpeed(inputData.IsSprint);
            }

            if (inMenu || InCustomization)
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
                    bool canSprint = isSprinting && !inventory.HasHomework && velocity >= data.MoveMaximumSpeed;
                    //Add other speed related logic. Boosters, slow when holding golden homework?
                    float maxMoveSpeed = canSprint ? data.SprintMaximumSpeed : data.MoveMaximumSpeed;
                    float sprintAcceleration =
                        (canSprint ? data.SprintAcceleration : data.SprintBraking) * Runner.DeltaTime;
                    currentMaxMoveSpeed = Mathf.MoveTowards(currentMaxMoveSpeed, maxMoveSpeed, sprintAcceleration);
                }
                else
                {
                    currentMaxMoveSpeed = Mathf.MoveTowards(currentMaxMoveSpeed, data.AimingMovementSpeed, data.AimingMovementDeceleration);
                }
            }
            else
            {
                currentMaxMoveSpeed = Mathf.MoveTowards(currentMaxMoveSpeed, data.MoveMaximumSpeed, data.SprintBraking);
            }
        }

        private void CalculateVelocity()
        {
            if (CanMove && HasMoveInput && !IsDashing && !isTurningAround)
            {
                velocity += data.MoveAcceleration * Runner.DeltaTime;
                velocity = Mathf.Min(velocity, currentMaxMoveSpeed);
            }
            else
            {
                velocity = Mathf.MoveTowards(velocity, 0, data.MoveDeceleration * Runner.DeltaTime);
            }
        }

        private void MovePlayer()
        {
            if (isTurningAround || isRagdoll) return;
            nRb.Rigidbody.velocity = transform.forward * velocity;
        }

        private IEnumerator RotatePlayerTowards()
        {
            if (IsDashing) yield break;

            float angle = Vector3.SignedAngle(transform.forward, lastMoveDirection, Vector3.up);
            if (Mathf.Abs(angle) > 179.5f) angle *= Random.Range(0, 2) * 2 - 1;
            Vector3 startRot = transform.eulerAngles;
            Vector3 endRot = startRot + Vector3.up * angle;
            float t = 0;
            while (t < 1)
            {
                float turnRateDivider = Mathf.Max(1, velocity - data.MoveMaximumSpeed);
                float turnRate = data.TurnRotationSpeed * Runner.DeltaTime / (turnRateDivider);
                if (isTurningAround) turnRate *= 5f;
                t += turnRate;
                transform.eulerAngles = Vector3.Lerp(startRot, endRot, t);
                yield return null;
            }
        }

        private void ResetVelocity()
        {
            velocity = 0;
            currentMaxMoveSpeed = data.MoveMaximumSpeed;
        }
    }
}