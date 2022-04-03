using System;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using Utilities.Extensions;
using Utilities.Unity;

namespace Units.Player
{
    public partial class PlayerEntity
    {
        public event Action<bool> OnDashAvailableChanged;

        public bool HasHitSomeoneThisFrame { get; private set; }
        public bool CanDash = true;
        public float RemainingTimeDashCoolDown => dashCooldown.ExpiredOrNotRunning(Runner) ? 0 : (dashCooldown.RemainingTime(Runner) ?? default(float));

        [Header("Dash")]
        [SerializeField] private Transform tacklePoint;

        [SerializeField] private bool ragdollOnDashMiss;

        [Networked (OnChanged = nameof(OnIsDashingChanged))] private NetworkBool IsDashing { get; set; } = false;

        private static void OnIsDashingChanged(Changed<PlayerEntity> changed)
        {
            changed.Behaviour.networkAnimator.Animator.SetBool(Dashing, changed.Behaviour.IsDashing);
        }

        private TickTimer dashTimer;
        private TickTimer dashCooldown;
        private bool hasHitSomeone;

        private readonly List<LagCompensatedHit> hits = new List<LagCompensatedHit>();
        private readonly List<LagCompensatedHit> collisions = new List<LagCompensatedHit>();

        private void DashUpdate()
        {
            HandleDashInput();
            if (IsDashing) DetectCollision();
            if (dashTimer.Expired(Runner)) OnHitNothing();
            if (dashCooldown.Expired(Runner)) ResetDashCoolDown();
        }

        private void ResetDashCoolDown()
        {
            OnDashAvailableChanged?.Invoke(true);
            CanDash = true;
        }

        private void HandleDashInput()
        {
            if (Inputs.IsDash && CanDash && !InMenu && !InCustomization) Dash();
        }

        private void Dash()
        {
            if (!CanMove || inventory.HasHomework || IsDashing) return;
            CanDash = false;
            OnDashAvailableChanged?.Invoke(false);
            IsDashing = true;
            hasHitSomeone = false;
            dashTimer = TickTimer.CreateFromSeconds(Runner, data.DashDuration);
            dashCooldown = TickTimer.CreateFromSeconds(Runner, data.DashCoolDown);
            Vector3 dirToTarget = GetDirToTarget();
            transform.forward = GetDashDirection(dirToTarget);
            CurrentSpeed = data.DashForce;

            if (Object.HasStateAuthority)
                RPC_DetectDashOnAllClients();
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_DetectDashOnAllClients()
        {
            PlayDashSoundLocally();
        }

        private Vector3 GetDirToTarget()
        {
            Vector3 dirToTarget = Vector3.zero;
            if (Runner.LagCompensation.OverlapSphere(transform.position, data.DashMaxAimAssistRange,
                    Object.InputAuthority, hits, Layers.GAMEPLAY_MASK) > 0)
            {
                float distanceToTarget = float.MaxValue;
                foreach (LagCompensatedHit hit in hits)
                {
                    if (hit.GameObject == gameObject) continue;
                    if (!hit.GameObject.IsAPlayerOrAI()) continue;

                    Vector3 dirToHit = (hit.GameObject.transform.position - transform.position).normalized;

                    if (!DirectionInViewAngle(dirToHit)) continue;

                    float distanceToHit = Vector3.Distance(transform.position, hit.GameObject.transform.position);

                    if (!HasLineOfSight(dirToHit, distanceToHit, out LagCompensatedHit obstacle)) continue;

                    if (hit.GameObject != obstacle.GameObject) continue;

                    if (!(distanceToHit < distanceToTarget)) continue;

                    distanceToTarget = distanceToHit;
                    dirToTarget = dirToHit;
                }
            }

            return dirToTarget;
        }

        private Vector3 GetDashDirection(Vector3 dirToTarget) => dirToTarget == Vector3.zero
            ? transform.forward
            : Vector3.Lerp(transform.forward, dirToTarget, data.DashAimAssistForce);

        private bool HasLineOfSight(Vector3 dirToHit, float distanceToHit, out LagCompensatedHit obstacle) =>
            Runner.LagCompensation.Raycast(transform.position, dirToHit, distanceToHit, Object.InputAuthority,
                out obstacle, Physics.AllLayers, HitOptions.IncludePhysX);

        private bool DirectionInViewAngle(Vector3 dirToHit) =>
            (Vector3.Angle(transform.forward, dirToHit) < data.DashAimAssistAngle);

        private void OnHitNothing()
        {
            if (!IsDashing) return;
            if (!hasHitSomeone)
            {
                print("Hit nothing");
                if (ragdollOnDashMiss) Hit(transform.forward);
            }
            IsDashing = false;
        }

        private void OnHitObject()
        {
            print("Hit Object");
            ResetVelocity();
            Hit(-transform.forward);
            IsDashing = false;
        }

        private TickTimer pushAnimTimer;

        private void OnHitOtherEntity(GameObject otherEntity)
        {
            print("Hit other entity");
            NetworkObject networkObject = otherEntity.GetComponentInEntity<NetworkObject>();
            Debug.Assert(networkObject, $"A player or an AI should have a {nameof(NetworkObject)}");
            if (pushAnimTimer.ExpiredOrNotRunning(Runner))
            {
                AnimationSetTrigger(Pushing);
                pushAnimTimer = TickTimer.CreateFromSeconds(Runner,0.533f);
            }
            RPC_GetHitAndDropItems(networkObject.Id, otherEntity.IsAPlayer(), transform.forward, data.DashForceApplied);
            if (Archetype != Archetype.Dasher)
            {
                ResetVelocity();
                IsDashing = false;
            }

            hasHitSomeone = true;
            HasHitSomeoneThisFrame = true;
        }

        private void DetectCollision()
        {
            if (Runner.LagCompensation.OverlapSphere(tacklePoint.position, data.DashDetectionSphereRadius,
                    Object.InputAuthority, collisions,
                    options: HitOptions.IncludePhysX) <= 0) return;
            
            Transform t = transform;
            LagCompensatedHit closestHit = FindClosestHit();

            GameObject go = closestHit.GameObject;
            if (!go) return;
                
            if (go.IsAPlayerOrAI())
            {
                OnHitOtherEntity(go);
            }
            else if (go.CompareTag(Tags.COLLIDABLE))
            {
                    
                Runner.GetPhysicsScene().Raycast(tacklePoint.position, t.forward, out RaycastHit info);
                if (Mathf.Abs(Vector3.Dot(info.normal, t.forward)) < 0.71)
                {
                    t.forward = Vector3.Reflect(t.forward, info.normal);
                }
                else
                {
                    OnHitObject();
                }
            }
        }
        
        private LagCompensatedHit FindClosestHit()
        {
            LagCompensatedHit closestHit = new LagCompensatedHit();
            float distance = float.MaxValue;
            foreach (LagCompensatedHit collision in collisions)
            {
                if (collision.GameObject == gameObject || collision.GameObject.transform.IsChildOf(gameObject.transform)) continue;
                
                float dst = Vector3.Distance(transform.position, collision.GameObject.transform.position);
                
                if (!(dst < distance)) continue;
                
                closestHit = collision;
                distance = dst;
            }
            return closestHit;
        }

        private void LateUpdate() => HasHitSomeoneThisFrame = false;

        private void OnDrawGizmosSelected()
        {
            if (Application.isPlaying)
            {
                //Tackle aim assist sphere + view angle
                Gizmos.color = Color.red;
                Vector3 pos = transform.position;
                Vector3 tPos = tacklePoint.position;
                Gizmos.DrawWireSphere(pos, data.DashMaxAimAssistRange);
                float y = transform.eulerAngles.y;
                float angleA = data.DashAimAssistAngle + y;
                float angleB = data.DashAimAssistAngle - y;
                Vector3 viewAngleA = new Vector3(Mathf.Sin(angleA * Mathf.Deg2Rad), 0, Mathf.Cos(angleA * Mathf.Deg2Rad));
                Vector3 viewAngleB = new Vector3(Mathf.Sin(-angleB * Mathf.Deg2Rad), 0, Mathf.Cos(-angleB * Mathf.Deg2Rad));

                Gizmos.DrawLine(pos, pos + viewAngleA * data.DashMaxAimAssistRange);
                Gizmos.DrawLine(pos, pos + viewAngleB * data.DashMaxAimAssistRange);

                //Tackled detection orb
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(tPos,data.DashDetectionSphereRadius);
                
                Gizmos.color = Color.black;
                Runner.GetPhysicsScene().Raycast(tPos, transform.forward, out RaycastHit info);
                Gizmos.DrawLine(tPos,info.point);
                Vector3 infoPos = info.transform.position;
                Gizmos.DrawLine(infoPos,infoPos + info.normal);
            }
        }
    }
}