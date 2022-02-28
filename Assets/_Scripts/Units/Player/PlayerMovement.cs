using System.Threading.Tasks;
using Fusion;
using Systems.Network;
using UnityEngine;
using Utilities.Extensions;
using Utilities.Unity;
using TickTimer = Utilities.TickTimer;

namespace Units.Player
{
    [RequireComponent(typeof(NetworkCharacterController))]
    public partial class PlayerEntity
    {
        private NetworkCharacterController cc;
        [Networked] private NetworkBool CanMove { get; set; } = true;
        [Networked] private NetworkBool Dashing { get; set; }

        [Networked] private Vector3 MoveDirection { get; set; } = Vector3.zero;

        private TickTimer dashTimer;

        private void MovementAwake()
        {
            cc = GetComponent<NetworkCharacterController>();
            cc.Config.MaxSpeed = data.MoveMaximumSpeed;
            cc.Config.Acceleration = data.MoveAcceleration;
            cc.Config.Braking = data.MoveDeceleration;
            dashTimer = new TickTimer(data.DashDuration);
            dashTimer.OnTimerEnd += () => EndDash();
        }

        private void MoveUpdate(NetworkInputData inputData)
        {
            GetInput(inputData);
            MovePlayer();
            if (Dashing) DetectCollision();
            RotatePlayer();
            dashTimer.Tick(Runner.DeltaTime);
        }

        private void GetInput(NetworkInputData inputData)
        {
            if (!Dashing)
            {
                MoveDirection = CanMove ? inputData.Move.V2ToFlatV3() : Vector3.zero;
                ChangeMoveSpeed(inputData.IsSprint);
                if (inputData.IsDash) Dash();
            }
        }

        private void ChangeMoveSpeed(bool isSprinting)
        {
            bool canSprint = isSprinting && !inventory.HasHomework;
            //Add other speed related logic. Boosters, slow when holding golden homework?
            cc.MaxSpeed = canSprint ? data.SprintMaximumSpeed : data.MoveMaximumSpeed;
        }

        private void Dash()
        {
            if (!CanMove || inventory.HasHomework || Dashing) return;
            Dashing = true;
            dashTimer.Reset();
            cc.MaxSpeed = data.DashSpeed;
            MoveDirection = transform.forward;
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
                    return;
                }
                EndDash();
            }
        }

        private async void Hit()
        {
            CanMove = false;
            await Task.Delay((int)(cc.MaxSpeed / data.MoveMaximumSpeed * data.KnockOutTimeInMS));
            CanMove = true;
        }

        private void MovePlayer()
        {
            cc.Move(MoveDirection);
        }

        private void RotatePlayer()
        {
            if (MoveDirection.sqrMagnitude > 0) transform.forward = MoveDirection;
        }
    }
}