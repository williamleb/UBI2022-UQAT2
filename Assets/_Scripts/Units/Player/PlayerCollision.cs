﻿using System.Collections;
using Fusion;
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
        
        private void Hit(Vector3 forceDirection = default, float forceMagnitude = default, float overrideHitDuration = -1f)
        {
            if (isImmune) return;

            ResetPlayerState();
            if (hitCoroutine != null)
            {
                StopCoroutine(hitCoroutine);
                hitCoroutine = null;
            }

            hitCoroutine = StartCoroutine(HitCoroutine(forceDirection, forceMagnitude, overrideHitDuration));
        }

        private int KnockOutTime => (int) (currentMaxMoveSpeed / data.MoveMaximumSpeed * data.KnockOutTimeInSeconds);
        
        private IEnumerator HitCoroutine(Vector3 forceDirection, float forceMagnitude, float overrideHitDuration)
        {
            CanMove = false;
            
            var delay = 
                overrideHitDuration > 0f ? overrideHitDuration : KnockOutTime;
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

            yield return Helpers.GetWait(delay - 1);

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
                if (collision.gameObject.CompareTag(Tags.COLLIDABLE))
                {
                    Debug.Log("Hit a wall");
                    ResetVelocity();
                    Hit(-f);
                }
                //Hit another player or AI
                else if (collision.gameObject.IsAPlayerOrAI())
                {
                    NetworkObject no = collision.gameObject.GetComponentInParent<NetworkObject>();
                    RPC_GetHitAndDropItems(no.Id, collision.gameObject.IsAPlayer(),f);
                    RPC_GetHitAndDropItems(Object.Id, true, -f);
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