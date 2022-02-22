using Fusion;
using Systems.Network;
using UnityEngine;

namespace Units.Player
{
    [RequireComponent(typeof(NetworkCharacterController))]
    public partial class PlayerEntity
    {
        [SerializeField] private Transform orientation;

        private NetworkCharacterController cc;

        [Networked] private Vector3 MoveDirection { get; set; } = Vector3.zero;

        private void MovementAwake()
        {
            cc = GetComponent<NetworkCharacterController>();
            /*cc.Config.MaxSpeed = data.MoveMaximumSpeed;
            cc.Config.Acceleration = data.MoveAcceleration;
            cc.Config.Braking = data.MoveDeceleration;
            cc.Config.AirControl = true;
            cc.Config.BaseJumpImpulse = data.JumpHeight;*/

        }

        private void MoveUpdate(NetworkInputData inputData)
        {
            GetInput(inputData);
            MovePlayer();
            RotatePlayer();
        }

        private void GetInput(NetworkInputData inputData)
        {
            MoveDirection = default;
            if (inputData.IsUp)
            {
                MoveDirection += Vector3.forward;
            }
            else if (inputData.IsDown)
            {
                MoveDirection += Vector3.back;
            }

            if (inputData.IsLeft)
            {
                MoveDirection += Vector3.left;
            }
            else if (inputData.IsRight)
            {
                MoveDirection += Vector3.right;
            }
        }
        
        private void MovePlayer()
        {
            cc.Move(MoveDirection);
        }

        private void RotatePlayer()
        {
            Vector3 ori = orientation.eulerAngles;
            orientation.rotation = Quaternion.Euler(ori);
        }
    }
}