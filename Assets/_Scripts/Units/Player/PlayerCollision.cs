using System.Threading.Tasks;
using Fusion;
using UnityEngine;
using Utilities.Extensions;

namespace Units.Player
{
    public partial class PlayerEntity
    {
        private async void Hit()
        {
            CanMove = false;
            int delay = (int) (currentMaxMoveSpeed / data.MoveMaximumSpeed * data.KnockOutTimeInMS);
            delay = Mathf.Max(1000, delay);
            await Task.Delay(delay - 1000);
            AnimGetUpTrigger(); //Only plays the animation if fallen
            await Task.Delay(1000);
            CanMove = true;
        }

        private void OnCollisionStay(Collision collision)
        {
            if (IsMovingFast)
            {
                Vector3 collisionDirection = (collision.contacts[0].point.Flat() - transform.position).normalized;
                // ReSharper disable once Unity.InefficientPropertyAccess
                float collisionDot = Vector3.Dot(transform.forward, collisionDirection);

                //We didn't hit it, it hit us and it will affect us
                if (!(collisionDot > 0.65)) return;

                //Hit a wall
                if (collision.gameObject.isStatic)
                {
                    ResetVelocity();
                    Hit();
                    AnimStumbleTrigger();
                }
                //Hit another player or AI
                else if (collision.gameObject.IsAPlayerOrAI())
                {
                    NetworkObject no = collision.gameObject.GetComponent<NetworkObject>();
                    RPC_GetHitAndDropItems(no.Id, collision.gameObject.IsAPlayer());
                    RPC_GetHitAndDropItems(Object.Id, true);
                }
            }
        }
    }
}