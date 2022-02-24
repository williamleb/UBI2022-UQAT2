using System.Collections;
using System.Threading.Tasks;
using Fusion;
using Systems.Network;
using Units.AI;
using UnityEngine;
using Utilities.Extensions;

namespace Units.Player
{
    [RequireComponent(typeof(NetworkCharacterController))]
    public partial class PlayerEntity
    {
        private NetworkCharacterController cc;
        [Networked] private NetworkBool CanMove { get; set; } = true;

        [Networked] private Vector3 MoveDirection { get; set; } = Vector3.zero;

        private void MovementAwake()
        {
            cc = GetComponent<NetworkCharacterController>();
            cc.Config.MaxSpeed = data.MoveMaximumSpeed;
            cc.Config.Acceleration = data.MoveAcceleration;
            cc.Config.Braking = data.MoveDeceleration;
        }

        private void MoveUpdate(NetworkInputData inputData)
        {
            GetInput(inputData);
            MovePlayer();
            RotatePlayer();
        }

        private void GetInput(NetworkInputData inputData)
        {
            MoveDirection = CanMove ? inputData.Move.V2ToFlatV3() : Vector3.zero;
            ChangeMoveSpeed(inputData.IsSprint);
            if (inputData.IsDash) Dash();
        }

        private void ChangeMoveSpeed(bool isSprinting)
        {
            bool canSprint = isSprinting && !inventory.HasHomework;
            //Add other speed related logic. Boosters, slow when golder homework?
            cc.MaxSpeed = canSprint ? data.SprintMaximumSpeed : data.MoveMaximumSpeed;
        }

        private void Dash()
        {
            if (!CanMove || inventory.HasHomework) return;
            Vector3 startPos = transform.position;
            cc.Velocity += transform.forward * (data.DashDistance * cc.MaxSpeed);
            cc.Move(Vector3.zero);
            //If hit something
            if (Runner.LagCompensation.Raycast(startPos, transform.forward, 1f,
                    Object.InputAuthority, out LagCompensatedHit hit))
            {
                var go = hit.GameObject;
                if (go.CompareTag(TAG) || go.CompareTag(AIEntity.TAG))
                {
                    var networkObject = go.GetComponent<NetworkObject>();
                    Debug.Assert(networkObject, $"A player or an AI should have a {nameof(NetworkObject)}");
                    RPC_DropItems(networkObject.Id, go.CompareTag(TAG));
                }
            }
            else
            {
                Hit();
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

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawLine(transform.position,transform.position + transform.forward * (data.DashDistance * cc.MaxSpeed));
        }
    }
}