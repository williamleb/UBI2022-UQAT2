using UnityEngine;

namespace Units.Player
{
    public partial class PlayerEntity
    {
        [SerializeField] private Transform orientation;
        [SerializeField] private Transform mainCamera;

        private CharacterController cc;
        private PlayerMovementData moveData;

        private Vector3 moveDirection = Vector3.zero;
        private Vector2 lookDelta;
        private Vector3 moveVelocity;
        private float fallVelocity;

        private bool jumpInput;
        private bool jumpImpulse;
        private float apexPoint;
        private float fallSpeed;
        private bool bufferJump;

        private void MovementAwake()
        {
            moveData = new PlayerMovementData(settings.PlayerMovementConfig);
            if (mainCamera == null) mainCamera = Camera.main.transform;
            cc = GetComponent<CharacterController>();
        }

        private void MoveUpdate()
        {
            GetInput();
            CalculateMovement();
            CalculateJumpApex();
            CalculateGravity();
            CalculateJump();
            MovePlayer();
            RotatePlayer();
        }

        private void GetInput()
        {
            moveDirection.Set(playerInputs.move.x, 0, playerInputs.move.y);
            lookDelta = playerInputs.look;
            if (!jumpInput && playerInputs.jump) jumpImpulse = true;
            jumpInput = playerInputs.jump;
            if (jumpImpulse && !cc.isGrounded && cc.velocity.y < 0) bufferJump = true;
        }

        private void CalculateMovement()
        {
            if (moveDirection.sqrMagnitude > 0)
            {
                float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg + mainCamera.eulerAngles.y;
                Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                moveVelocity += moveDir * (moveData.Acceleration * Time.deltaTime);
                moveVelocity = Vector3.ClampMagnitude(moveVelocity, moveData.MaximumSpeed);

                if (!cc.isGrounded)
                {
                    moveVelocity += moveDir * (moveData.ApexBonusControl * apexPoint * Time.deltaTime);
                }

            }
            else
            {
                moveVelocity = Vector3.MoveTowards(moveVelocity, Vector3.zero, moveData.Deceleration * Time.deltaTime);
            }
        }

        private void CalculateJumpApex()
        {
            if (!cc.isGrounded)
            {
                apexPoint = Mathf.InverseLerp(moveData.JumpApexThreshold, 0, Mathf.Abs(fallVelocity));
                fallSpeed = Mathf.Lerp(moveData.MinFallAcceleration, moveData.MaxFallAcceleration, apexPoint);
            }
            else
            {
                apexPoint = 0;
            }
        }

        private void CalculateGravity()
        {
            if (cc.isGrounded)
            {
                if (fallVelocity < 0) fallVelocity = -1;
            }
            else
            {
                if (!jumpInput && fallVelocity > 0)
                {
                    fallVelocity -= fallSpeed * moveData.JumpEndEarlyGravityModifier * Time.deltaTime;
                }
                else
                {
                    fallVelocity -= fallSpeed * Time.deltaTime;
                }

                if (fallVelocity < moveData.MaxFallSpeed) fallVelocity = moveData.MaxFallSpeed;
            }
        }

        private void CalculateJump()
        {
            if ((jumpImpulse || bufferJump) && cc.isGrounded)
            {
                jumpImpulse = false;
                bufferJump = false;
                fallVelocity = moveData.JumpHeight;
            }
        }

        private void MovePlayer()
        { 
            cc.Move(new Vector3(moveVelocity.x,fallVelocity,moveVelocity.z) * Time.deltaTime);
        }

        private void RotatePlayer()
        {
            var ori = orientation.eulerAngles;
            ori.x = Mathf.Clamp(ori.x - lookDelta.y * moveData.MouseSensitivity, 1,75);
            ori.y += lookDelta.x * moveData.MouseSensitivity;
            ori.z = 0;
            orientation.rotation = Quaternion.Euler(ori);
        }

        private void StopMoving()
        {
            cc.Move(Vector3.zero);
        }
    }
}