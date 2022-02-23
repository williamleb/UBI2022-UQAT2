using System.Collections;
using System.Threading.Tasks;
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
        private bool canMove = true;

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
            if (canMove) MovePlayer();
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
            cc.MaxSpeed = inputData.IsSprint && !inventory.HasHomework ? data.SprintMaximumSpeed : data.MoveMaximumSpeed;
            if (inputData.IsDash && !inventory.HasHomework) StartCoroutine(Dash());
        }

        private IEnumerator Dash()
        {
            //TODO validate what the solution is for the tanks hitting each other
            RaycastHit[] hits = new RaycastHit[1];
            for (int i = 0; i < data.DashDistance; i++)
            {
                cc.Move(MoveDirection);
                if (Runner.GetPhysicsScene().Raycast(transform.position, transform.forward, hits, 0.3f,
                        Physics.AllLayers) > 0)
                {
                    PlayerEntity other = hits[0].collider.gameObject.GetComponent<PlayerEntity>();
                    if (other)
                    {
                        other.Hit();
                        yield break;
                    }
                }
                yield return null;
            }

            Hit();
        }

        private async void Hit()
        {
            inventory.DropEverything();
            canMove = false;
            await Task.Delay((int)(cc.MaxSpeed / data.MoveMaximumSpeed * data.KnockOutTimeInMS));
            canMove = true;
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