using System;
using System.Threading.Tasks;
using Fusion;
using Systems.Network;
using UnityEngine;
using Utilities.Extensions;
using Utilities.Unity;
using TickTimer = Utilities.TickTimer;

namespace Units.Player
{
    public partial class PlayerEntity
    {
        private NetworkRigidbody nRb;
        [Networked] private NetworkBool CanMove { get; set; } = true;
        [Networked] private NetworkBool Dashing { get; set; }

        [Networked] private Vector3 MoveDirection { get; set; } = Vector3.zero;

        private TickTimer dashTimer;
        private bool hasHitSomeoneThisFrame = false;
        private float maxSpeed;
        private Vector3 velocity = Vector3.zero;

        public bool HasHitSomeoneThisFrame => hasHitSomeoneThisFrame;

        private float CurrentAcceleration
        {
            get
            {
                //turn rate decreases acceleration, acceleration changes move velocity
                float turnRate = data.TurnRate * (maxSpeed / data.MoveMaximumSpeed);
                //If velocity and move direction are not aligned the acceleration is reduced
                //dot returns 1 when vectors are aligned and 0 when perpendicular
                turnRate -= Vector3.Dot(velocity.normalized, MoveDirection);
                return data.MoveAcceleration / turnRate;
            }
        }

        private void MovementAwake()
        {
            nRb = GetComponent<NetworkRigidbody>();
            dashTimer = new TickTimer(data.DashDuration);
            dashTimer.OnTimerEnd += () => EndDash();
        }

        private void MoveUpdate()
        {
            CalculateVelocity();
            MovePlayer();
            if (Dashing) DetectCollision();
            RotatePlayer();
            dashTimer.Tick(Runner.DeltaTime);
        }


        private void SetMoveInput(NetworkInputData inputData)
        {
            if (!Dashing)
            {
                MoveDirection = CanMove ? inputData.Move.V2ToFlatV3() : Vector3.zero;
                //I don't want to use normalize since I want the magnitude to be smaller than 1 sometimes 
                MoveDirection = Vector3.ClampMagnitude(MoveDirection, 1);
                ChangeMoveSpeed(inputData.IsSprint);
                if (inputData.IsDash) Dash();
            }
        }

        private void ChangeMoveSpeed(bool isSprinting)
        {
            bool canSprint = isSprinting && !inventory.HasHomework;
            //Add other speed related logic. Boosters, slow when holding golden homework?
            maxSpeed = canSprint ? data.SprintMaximumSpeed : data.MoveMaximumSpeed;
        }

        private void Dash()
        {
            if (!CanMove || inventory.HasHomework || Dashing) return;
            Dashing = true;
            dashTimer.Reset();
            maxSpeed = data.DashForce;
            //TODO add dash animation
        }

        private void CalculateVelocity()
        {
            //Reset local velocity if character isn't moving.
            if (nRb.Rigidbody.velocity.sqrMagnitude < 0.1) velocity = Vector3.zero;
            
            if (MoveDirection.sqrMagnitude > 0)
            {
                velocity += MoveDirection * (CurrentAcceleration * Runner.DeltaTime);
                velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
            }
            else
            {
                velocity = Vector3.MoveTowards(velocity, Vector3.zero, data.MoveDeceleration * Runner.DeltaTime);
            }
        }

        private void EndDash(bool knockOutPlayer = true)
        {
            if (knockOutPlayer && Dashing)
            {
                Hit();
            }
            Dashing = false;

        }

        private void DetectCollision()
        {
            if (Runner.LagCompensation.Raycast(transform.position, transform.forward, 0.5f, Object.InputAuthority, out LagCompensatedHit hit,Physics.AllLayers,HitOptions.IncludePhysX))
            {
                var go = hit.GameObject;
                if (go.CompareTag(Tags.PLAYER) || go.CompareTag(Tags.AI))
                {
                    var networkObject = go.GetComponentInEntity<NetworkObject>();
                    Debug.Assert(networkObject, $"A player or an AI should have a {nameof(NetworkObject)}");
                    RPC_DropItems(networkObject.Id, go.CompareTag(Tags.PLAYER));
                    EndDash(false);
                    hasHitSomeoneThisFrame = true;
                    return;
                }
                EndDash();
            }
        }

        private void LateUpdate()
        {
            hasHitSomeoneThisFrame = false;
        }

        private async void Hit()
        {
            CanMove = false;
            await Task.Delay((int)(maxSpeed / data.MoveMaximumSpeed * data.KnockOutTimeInMS));
            CanMove = true;
        }

        private void MovePlayer()
        {
            nRb.Rigidbody.velocity = velocity;
        }

        private void RotatePlayer()
        {
            if (MoveDirection.sqrMagnitude > 0)
            {
                transform.forward = Vector3.MoveTowards(transform.forward, MoveDirection, data.TurnRotationSpeed);
            }
        }
    }
}