using Fusion;
using Systems.Network;
using UnityEngine;
using Utilities.Extensions;

namespace Units.Player
{
    public partial class PlayerEntity
    {

        private NetworkRigidbody nRb;
        [Networked] private NetworkBool CanMove { get; set; } = true;
        [Networked] private Vector3 MoveDirection { get; set; } = Vector3.zero;
        
        private float currentMaxMoveSpeed;
        private Vector3 velocity = Vector3.zero;
        private Vector3 lastMoveDirection = Vector3.zero;

        private float CurrentMoveSpeed => nRb.Rigidbody.velocity.magnitude;
        private bool HasMoveInput => MoveDirection.sqrMagnitude > 0.05;
        private bool IsPlayerMoving => nRb.Rigidbody.velocity.sqrMagnitude > 0.05;
        private bool IsMovingFast => CurrentMoveSpeed >= data.SprintFumbleThreshold * data.SprintMaximumSpeed;
        
        private float CurrentAcceleration
        {
            get
            {
                //turn rate decreases acceleration, acceleration changes move velocity
                float turnRate = data.TurnRate * (currentMaxMoveSpeed / data.MoveMaximumSpeed);
                //If velocity and move direction are not aligned the acceleration is reduced
                //dot returns 1 when vectors are aligned and 0 when perpendicular
                turnRate -= Vector3.Dot(velocity.normalized, MoveDirection);
                return data.MoveAcceleration / turnRate;
            }
        }

        private void MovementAwake()
        {
            nRb = GetComponent<NetworkRigidbody>();
        }

        private void MoveUpdate(NetworkInputData inputData)
        {
            HandleMoveInput(inputData);
            CalculateVelocity();
            MovePlayer();
            RotatePlayer();
        }


        private void HandleMoveInput(NetworkInputData inputData)
        {
            if (!IsDashing)
            {
                MoveDirection = CanMove ? inputData.Move.V2ToFlatV3() : Vector3.zero;
                if (HasMoveInput) lastMoveDirection = MoveDirection;
                //I don't want to use normalize since I want the magnitude to be smaller than 1 sometimes 
                MoveDirection = Vector3.ClampMagnitude(MoveDirection, 1);
                ChangeMoveSpeed(inputData.IsSprint);
            }
        }

        private void ChangeMoveSpeed(bool isSprinting)
        {
            if (HasMoveInput)
            {
                bool canSprint = isSprinting && !inventory.HasHomework;
                //Add other speed related logic. Boosters, slow when holding golden homework?
                float maxMoveSpeed = canSprint ? data.SprintMaximumSpeed : data.MoveMaximumSpeed;
                float sprintAcceleration = (canSprint ? data.SprintAcceleration : data.SprintBraking) * Runner.DeltaTime;
                currentMaxMoveSpeed = Mathf.MoveTowards(currentMaxMoveSpeed, maxMoveSpeed, sprintAcceleration);   
            }
            else
            {
                currentMaxMoveSpeed = data.MoveMaximumSpeed;
            }
        }
        
        private void CalculateVelocity()
        {
            if (HasMoveInput && !IsDashing)
            {
                if (!IsPlayerMoving) ResetVelocity();
                
                velocity += MoveDirection * (CurrentAcceleration * Runner.DeltaTime);
                velocity = Vector3.ClampMagnitude(velocity, currentMaxMoveSpeed);
            }
            else
            {
                velocity = Vector3.MoveTowards(velocity, Vector3.zero, data.MoveDeceleration * Runner.DeltaTime);
            }
        }

        private void MovePlayer()
        {
            nRb.Rigidbody.velocity = velocity;
        }

        private void RotatePlayer()
        {
            transform.forward = Vector3.RotateTowards(transform.forward, lastMoveDirection, data.TurnRotationSpeed,data.TurnRotationSpeed);
        }
        
        private void ResetVelocity()
        {
            velocity = Vector3.zero;
        }
    }
}