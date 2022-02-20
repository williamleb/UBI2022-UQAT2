using Fusion;
using Systems.Network;
using UnityEngine;
using Utilities.Extensions;

namespace Units.Player
{
    public partial class PlayerEntity
    {
        [SerializeField] private Transform orientation;
        [SerializeField] private Transform mainCamera;

        private NetworkCharacterController cc;

        [Networked] private Vector2 MoveDirection { get; set; } = Vector2.zero;
        [Networked] private Vector2 LookDelta { get; set; }

        private NetworkBool jumpInput;
        private NetworkBool jumpImpulse;
        private float apexPoint;
        private float fallSpeed;
        private NetworkBool bufferJump;

        private void MovementAwake()
        {
            if (mainCamera == null && Camera.main != null) mainCamera = Camera.main.transform;
            cc = GetComponent<NetworkCharacterController>();
            cc.Config.MaxSpeed = data.MoveMaximumSpeed;
            cc.Config.Acceleration = data.MoveAcceleration;
            cc.Config.Braking = data.MoveDeceleration;
            cc.Config.AirControl = true;
            cc.Config.BaseJumpImpulse = data.JumpHeight;

        }

        private void MoveUpdate(NetworkInputData inputData)
        {
            GetInput(inputData);
            CalculateJumpApex();
            CalculateGravity();
            CalculateJump();
            MovePlayer();
            RotatePlayer();
        }

        private void GetInput(NetworkInputData inputData)
        {
            MoveDirection = inputData.Move;
            LookDelta = inputData.Look;
            if (!jumpInput && inputData.IsJump) jumpImpulse = true;
            jumpInput = inputData.IsJump;
            if (jumpImpulse && !cc.Grounded && cc.Velocity.y < 0) bufferJump = true;
        }

        private void CalculateJumpApex()
        {
            if (!cc.Grounded)
            {
                apexPoint = Mathf.InverseLerp(data.JumpApexThreshold, 0, Mathf.Abs(cc.Velocity.y));
                fallSpeed = Mathf.Lerp(data.MinFallAcceleration, data.MaxFallAcceleration, apexPoint);
            }
            else
            {
                apexPoint = 0;
            }
        }

        private void CalculateGravity()
        {
            if (cc.Grounded)
            {
                if (cc.Velocity.y < 0) cc.Velocity = Vector3.up * -0.1f;
            }
            else
            {
                if (!jumpInput && cc.Velocity.y > 0)
                {
                    cc.Velocity -= Vector3.up * fallSpeed * data.JumpEndEarlyGravityModifier * Time.deltaTime;
                }
                else
                {
                    cc.Velocity -= Vector3.up * fallSpeed * Time.deltaTime;
                }

                if (cc.Velocity.y < data.MaxFallSpeed) cc.Velocity += Vector3.up * (data.MaxFallSpeed - cc.Velocity.y);
            }
        }

        private void CalculateJump()
        {
            if ((jumpImpulse || bufferJump) && cc.Grounded)
            {
                jumpImpulse = false;
                bufferJump = false;
                cc.Jump();
            }
        }

        private void MovePlayer()
        {
            cc.Move(MoveDirection.V2ToFlatV3());
        }

        private void RotatePlayer()
        {
            Vector3 ori = orientation.eulerAngles;
            ori.x = Mathf.Clamp(ori.x - LookDelta.y * data.MouseSensitivity, 1, 75);
            ori.y += LookDelta.x * data.MouseSensitivity;
            ori.z = 0;
            orientation.rotation = Quaternion.Euler(ori);
        }
    }
}