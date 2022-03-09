﻿using System.Collections.Generic;
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

        [SerializeField] private Transform tacklePoint;

        [Networked] private NetworkBool IsDashing { get; set; } = false;

        private TickTimer dashTimer;
        private TickTimer dashCooldown;
        private bool hasHitSomeoneThisFrame;
        private bool canDash = true;
        
        private readonly List<LagCompensatedHit> hits = new List<LagCompensatedHit>();
        private readonly List<LagCompensatedHit> collisions = new List<LagCompensatedHit>();

        private void DashAwake()
        {
            dashTimer = new TickTimer(data.DashDuration, false);
            dashCooldown = new TickTimer(data.DashCoolDown, false);
            dashTimer.OnTimerEnd += OnHitNothing;
            dashCooldown.OnTimerEnd += ResetDashCoolDown;
        }


        private void DashUpdate(NetworkInputData inputData)
        {
            HandleDashInput(inputData);
            if (IsDashing) DetectCollision();
            dashTimer.Tick(Runner.DeltaTime);
            dashCooldown.Tick(Runner.DeltaTime);
        }
        private void ResetDashCoolDown()
        {
            canDash = true;
        }

        private void HandleDashInput(NetworkInputData inputData)
        {
            if (inputData.IsDash && canDash) Dash();
        }

        private void Dash()
        {
            if (!CanMove || inventory.HasHomework || IsDashing) return;
            canDash = false;
            IsDashing = true;
            dashTimer.Reset();
            dashCooldown.Reset();
            Vector3 dirToTarget = GetDirToTarget();
            velocity = GetDashDirection(dirToTarget) * data.DashForce;
        }
        
        private Vector3 GetDirToTarget()
        {
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
            if (!IsDashing) return;
            print("Hit nothing");
            AnimFallTrigger();
            Hit();
            IsDashing = false;
        }

        private void OnHitObject()
        {
            print("Hit Object");
            Hit();
            ResetVelocity();
            //TODO activate ragdoll
            AnimStumbleTrigger();
            IsDashing = false;
        }

        private void OnHitOtherEntity(GameObject otherEntity)
        {
            print("Hit other entity");
            NetworkObject networkObject = otherEntity.GetComponentInEntity<NetworkObject>();
            Debug.Assert(networkObject, $"A player or an AI should have a {nameof(NetworkObject)}");
            RPC_GetHitAndDropItems(networkObject.Id, otherEntity.CompareTag(Tags.PLAYER));
            hasHitSomeoneThisFrame = true;
            IsDashing = false;
            ResetVelocity();
        }

        private void DetectCollision()
        {
            GameObject closestHit = null;
            float distance = float.MaxValue;
            if (Runner.LagCompensation.OverlapSphere(tacklePoint.position,1,Object.InputAuthority,collisions, options:HitOptions.IncludePhysX) > 0)
            {
                foreach (LagCompensatedHit collision in collisions)
                {
                    if (collision.GameObject == gameObject) continue;
                    float dst = Vector3.Distance(transform.position, collision.GameObject.transform.position);
                    if (dst < distance)
                    {
                        closestHit = collision.GameObject;
                        distance = dst;
                    }
                }

                if (!closestHit) return;
                
                if (closestHit.IsAPlayerOrAI())
                {
                    OnHitOtherEntity(closestHit);
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