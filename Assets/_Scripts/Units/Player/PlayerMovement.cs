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

        private Vector3 moveDirection = Vector3.zero;
        private Vector2 lookDelta;
        private Vector3 moveVelocity;
        private float yVelocity;

        private bool jumpInput;
        private bool jumpImpulse;
        private float apexPoint;
        private float fallSpeed;
        private bool bufferJump;

        private void MovementAwake()
        {
            if (mainCamera == null && Camera.main != null) mainCamera = Camera.main.transform;
            cc = GetComponent<NetworkCharacterController>();
        }

        private void MoveUpdate(NetworkInputData inputData)
        {
            GetInput(inputData);
            CalculateMovement();
            CalculateJumpApex();
            CalculateGravity();
            CalculateJump();
            MovePlayer();
            RotatePlayer();
        }

        private void GetInput(NetworkInputData inputData)
        {
            moveDirection.Set(inputData.Move.x, 0, inputData.Move.y);
            lookDelta = inputData.Look;
            if (!jumpInput && inputData.IsJump) jumpImpulse = true;
            jumpInput = inputData.IsJump;
            if (jumpImpulse && !cc.Grounded && cc.Velocity.y < 0) bufferJump = true;
        }

        private void CalculateMovement()
        {
            if (moveDirection.sqrMagnitude > 0)
            {
                float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg +
                                    mainCamera.eulerAngles.y;
                Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                moveVelocity += moveDir * (data.MoveAcceleration * Time.deltaTime);
                moveVelocity = Vector3.ClampMagnitude(moveVelocity, data.MoveMaximumSpeed);

                if (!cc.Grounded)
                {
                    moveVelocity += moveDir * (data.MoveAirBonusControl * apexPoint * Time.deltaTime);
                }
            }
            else
            {
                moveVelocity = Vector3.MoveTowards(moveVelocity, Vector3.zero, data.MoveDeceleration * Time.deltaTime);
            }
        }

        private void CalculateJumpApex()
        {
            if (!cc.Grounded)
            {
                apexPoint = Mathf.InverseLerp(data.JumpApexThreshold, 0, Mathf.Abs(yVelocity));
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
                if (yVelocity < 0) yVelocity = -0.1f;
            }
            else
            {
                if (!jumpInput && yVelocity > 0)
                {
                    yVelocity -= fallSpeed * data.JumpEndEarlyGravityModifier * Time.deltaTime;
                }
                else
                {
                    yVelocity -= fallSpeed * Time.deltaTime;
                }

                if (yVelocity < data.MaxFallSpeed) yVelocity = data.MaxFallSpeed;
            }
        }

        private void CalculateJump()
        {
            if ((jumpImpulse || bufferJump) && cc.Grounded)
            {
                jumpImpulse = false;
                bufferJump = false;
                yVelocity = data.JumpHeight;
            }
        }

        private void MovePlayer()
        {
            cc.Move(moveVelocity.Flat() * Runner.DeltaTime);
            cc.Jump(impulse:yVelocity);
        }

        private void RotatePlayer()
        {
            Vector3 ori = orientation.eulerAngles;
            ori.x = Mathf.Clamp(ori.x - lookDelta.y * data.MouseSensitivity, 1, 75);
            ori.y += lookDelta.x * data.MouseSensitivity;
            ori.z = 0;
            orientation.rotation = Quaternion.Euler(ori);
        }
    }
}