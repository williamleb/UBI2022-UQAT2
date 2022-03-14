using System.Collections;
using Fusion;
using UnityEngine;
using Utilities.Extensions;
using Utilities.Unity;

namespace Units.Player
{
    public partial class PlayerEntity
    {
        // forceDirection : direction in which the hit acts on the player
        // forceMagnitude : magnitude of the force that acts on the player. Default = network rigidbody velocity

        private Coroutine hitCoroutine;
        
        private void Hit(Vector3 forceDirection = default, float forceMagnitude = default)
        {
            if (hitCoroutine != null)
            {
                StopCoroutine(hitCoroutine);
                hitCoroutine = null;
            }

            hitCoroutine = StartCoroutine(HitCoroutine(forceDirection, forceMagnitude));
        }

        private IEnumerator HitCoroutine(Vector3 forceDirection, float forceMagnitude)
        {
            CanMove = false;
            int delay = (int) (currentMaxMoveSpeed / data.MoveMaximumSpeed * data.KnockOutTimeInSeconds);
            delay = Mathf.Max(2, delay);

            if (Object.HasStateAuthority)
            {
                IsGettingUpB = false;
                IsGettingUpF = false;
                
                if (forceDirection != default)
                {
                    RPC_ToggleRagdoll(true, forceDirection, forceMagnitude);
                }
                else
                {
                    RPC_ToggleRagdoll(true);
                }
            }

            yield return new WaitForSeconds(delay - 1);

            transform.position = ragdollTransform.position.Flat();
            yield return new WaitForEndOfFrame();

            if (Object.HasStateAuthority)
            {
                RPC_ToggleRagdoll(false);
                IsGettingUpF = Vector3.Dot(ragdollPelvis.forward, Vector3.up) > 0;
                IsGettingUpB = !IsGettingUpF;
                AnimationUpdate();
            }

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

                //Hit a wall or other collidable
                if (collision.gameObject.CompareTag(Tags.COLLIDABLE))
                {
                    Debug.Log("Hit a wall");
                    ResetVelocity();
                    Hit(-transform.forward);
                }
                //Hit another player or AI
                else if (collision.gameObject.IsAPlayerOrAI())
                {
                    NetworkObject no = collision.gameObject.GetComponent<NetworkObject>();
                    RPC_GetHitAndDropItems(no.Id, collision.gameObject.IsAPlayer(),transform.forward);
                    RPC_GetHitAndDropItems(Object.Id, true, -transform.forward);
                }
            }
        }
    }
}