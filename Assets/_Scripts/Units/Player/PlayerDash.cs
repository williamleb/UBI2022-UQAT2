using System.Collections.Generic;
using Fusion;
using Systems.Network;
using UnityEngine;
using Utilities.Extensions;
using Utilities.Unity;
using TickTimer = Utilities.TickTimer;

namespace Units.Player
{
    public partial class PlayerEntity
    {
        public bool HasHitSomeoneThisFrame => hasHitSomeoneThisFrame;

        [Networked] private NetworkBool IsDashing { get; set; } = false;

        private TickTimer dashTimer;
        private bool hasHitSomeoneThisFrame;

        private void DashAwake()
        {
            dashTimer = new TickTimer(data.DashDuration);
            dashTimer.OnTimerEnd += OnHitNothing;
        }

        private void DashUpdate(NetworkInputData inputData)
        {
            HandleDashInput(inputData);
            if (IsDashing) DetectCollision();
            dashTimer.Tick(Runner.DeltaTime);
        }

        private void HandleDashInput(NetworkInputData inputData)
        {
            if (inputData.IsDash) Dash();
        }

        private void Dash()
        {
            if (!CanMove || inventory.HasHomework || IsDashing) return;
            IsDashing = true;
            dashTimer.Reset();
            Vector3 dirToTarget = GetDirToTarget();
            velocity = GetDashDirection(dirToTarget) * data.DashForce;
        }

        private Vector3 GetDirToTarget()
        {
            List<LagCompensatedHit> hits = new List<LagCompensatedHit>();
            Vector3 dirToTarget = Vector3.zero;
            if (Runner.LagCompensation.OverlapSphere(transform.position, data.DashMaxAimAssistRange, Object.InputAuthority, hits) > 0)
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

        private Vector3 GetDashDirection(Vector3 dirToTarget) => dirToTarget == Vector3.zero ? transform.forward : Vector3.Lerp(transform.forward, dirToTarget, data.DashAimAssistForce);
        private bool HasLineOfSight(Vector3 dirToHit, float distanceToHit, out LagCompensatedHit obstacle) => Runner.LagCompensation.Raycast(transform.position, dirToHit, distanceToHit, Object.InputAuthority, out obstacle, Physics.AllLayers, HitOptions.IncludePhysX);
        private bool DirectionInViewAngle(Vector3 dirToHit) => (Vector3.Angle(transform.forward, dirToHit) < data.DashAimAssistAngle);

        private void OnHitNothing()
        {
            IsDashing = false;
            Hit();
            AnimFallTrigger();
        }

        private void OnHitObject()
        {
            IsDashing = false;
            Hit();
            ResetVelocity();
            //TODO activate ragdoll
            AnimStumbleTrigger();
        }

        private void OnHitOtherEntity(GameObject otherEntity)
        {
            NetworkObject networkObject = otherEntity.GetComponentInEntity<NetworkObject>();
            Debug.Assert(networkObject, $"A player or an AI should have a {nameof(NetworkObject)}");
            RPC_GetHitAndDropItems(networkObject.Id, otherEntity.CompareTag(Tags.PLAYER));
            hasHitSomeoneThisFrame = true;
            IsDashing = false;
            ResetVelocity();
        }

        private void DetectCollision()
        {
            if (Runner.LagCompensation.Raycast(transform.position + Vector3.up, transform.forward, 1f,
                    Object.InputAuthority, out LagCompensatedHit hit, Physics.AllLayers, HitOptions.IncludePhysX))
            {
                GameObject go = hit.GameObject;
                if (go.CompareTag(Tags.PLAYER) || go.CompareTag(Tags.AI))
                {
                    OnHitOtherEntity(go);
                }
                else
                {
                    OnHitObject();
                }

            }
        }

        private void LateUpdate()
        {
            hasHitSomeoneThisFrame = false;
        }

        private void OnDrawGizmosSelected()
        {
            if (Application.isPlaying)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, data.DashMaxAimAssistRange);
                Vector3 viewAngleA = new Vector3(Mathf.Sin(data.DashAimAssistAngle * Mathf.Deg2Rad), 0,
                    Mathf.Cos(data.DashAimAssistAngle * Mathf.Deg2Rad));
                Vector3 viewAngleB = new Vector3(Mathf.Sin(-data.DashAimAssistAngle * Mathf.Deg2Rad), 0,
                    Mathf.Cos(-data.DashAimAssistAngle * Mathf.Deg2Rad));
                Gizmos.DrawLine(transform.position, transform.position + viewAngleA * data.DashMaxAimAssistRange);
                Gizmos.DrawLine(transform.position, transform.position + viewAngleB * data.DashMaxAimAssistRange);
            }
        }
    }
}