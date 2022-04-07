using System.Collections;
using Fusion;
using Systems;
using UnityEngine;
using Utilities;
using Utilities.Extensions;
using Utilities.Unity;

namespace Units.Player
{
    public partial class PlayerEntity
    {
        // forceDirection : direction in which the hit acts on the player
        // forceMagnitude : magnitude of the force that acts on the player. Default = network rigidbody velocity
        
        private Coroutine hitCoroutine;
        private RumbleKey collisionRumbleKey;

        private void InitCollision()
        {
            collisionRumbleKey = new RumbleKey(this);
        }
        
        private void Hit(Vector3 forceDirection = default, float forceMagnitude = default, float overrideHitDuration = -1f, bool fumble = false)
        {
            if (isImmune) return;

            ResetPlayerState();
            if (hitCoroutine != null)
            {
                StopCoroutine(hitCoroutine);
                hitCoroutine = null;
            }

            hitCoroutine = StartCoroutine(HitCoroutine(forceDirection, forceMagnitude, overrideHitDuration, fumble));
        }

        private int KnockOutTime => (int) (currentMaxMoveSpeed / data.MoveMaximumSpeed * data.KnockOutTimeInSeconds);
        private int FumbleKnockOutTime => (int) (currentMaxMoveSpeed / data.MoveMaximumSpeed * data.FumbleKnockOutTimeInSeconds);
        
        private IEnumerator HitCoroutine(Vector3 forceDirection, float forceMagnitude, float overrideHitDuration, bool fumble)
        {
            CanMove = false;
            RumbleSystem.Instance.SetRumbleIfUsingController(collisionRumbleKey, 1, 1, IsUsingGamePad);
            var delay = 
                overrideHitDuration > 0f ? overrideHitDuration : fumble ? FumbleKnockOutTime : KnockOutTime;
            delay = Mathf.Max(2, delay);

            if (Object.HasStateAuthority)
            {
                if (forceDirection != default)
                {
                    RPC_ToggleRagdoll(true, forceDirection, forceMagnitude);
                }
                else
                {
                    RPC_ToggleRagdoll(true);
                }
            }

            yield return Helpers.GetWait((delay - 1) * 0.25f); 
            RumbleSystem.Instance.StopRumble(collisionRumbleKey);
            yield return Helpers.GetWait((delay - 1) * 0.75f);
            
            transform.position = ragdollTransform.position.Flat();

            if (Object.HasStateAuthority)
            {
                RPC_ToggleRagdoll(false);
            }
        }

        private void OnCollisionStay(Collision collision)
        {
            if (!Object || IsDashing) return;
            
            if (IsMovingFast)
            {
                Transform t = transform;
                Vector3 f = t.forward;
                Vector3 collisionDirection = (collision.contacts[0].point.Flat() - t.position).normalized;
                // ReSharper disable once Unity.InefficientPropertyAccess
                float collisionDot = Vector3.Dot(f, collisionDirection);

                //We didn't hit it, it hit us and it will affect us
                if (!(collisionDot > 0.65)) return;

                //Hit a wall or other collidable
                if (collision.gameObject.CompareTag(Tags.COLLIDABLE) && data.CanFumble)
                {
                    Debug.Log("Hit a wall");
                    ResetVelocity();
                    Hit(-f, fumble: true);
                }
                //Hit another player or AI
                else if (collision.gameObject.IsAPlayerOrAI())
                {
                    NetworkObject no = collision.gameObject.GetComponentInParent<NetworkObject>();
                    RPC_GetHitAndDropItems(no.Id, collision.gameObject.IsAPlayer(),f, fumble: true);
                    if (data.CanFumble)
                        RPC_GetHitAndDropItems(Object.Id, true, -f, fumble: true);
                }
            }
            else
            {
                if (collision.gameObject.CompareTag(Tags.COLLIDABLE) || collision.gameObject.IsAPlayerOrAI())
                    isOnWallTimer = TickTimer.CreateFromSeconds(Runner,0.1f);
            }
        }
    }
}