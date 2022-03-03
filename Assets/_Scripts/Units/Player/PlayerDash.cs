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
        
        [Networked] private NetworkBool IsDashing { get; set; }
        
        private TickTimer dashTimer;
        private bool hasHitSomeoneThisFrame;
        
        private void DashAwake()
        {
            dashTimer = new TickTimer(data.DashDuration);
            dashTimer.OnTimerEnd += () => EndDash();
        }

        private void DashUpdate()
        {
            if (IsDashing) DetectCollision();
            dashTimer.Tick(Runner.DeltaTime);
        }

        private void SetDashInput(NetworkInputData inputData)
        {
            if (inputData.IsDash) Dash();
        }

        private void Dash()
        {
            if (!CanMove || inventory.HasHomework || IsDashing) return;
            IsDashing = true;
            dashTimer.Reset();
            velocity = transform.forward * data.DashForce;
            //TODO add dash animation
        }
        
        private void EndDash(bool knockOutPlayer = true)
        {
            if (knockOutPlayer && IsDashing)
            {
                Hit();
            }
            IsDashing = false;

        }
        
        private void DetectCollision()
        {
            if (Runner.LagCompensation.Raycast(transform.position, transform.forward, 0.5f, Object.InputAuthority, out LagCompensatedHit hit,Physics.AllLayers,HitOptions.IncludePhysX))
            {
                var go = hit.GameObject;
                if (go.CompareTag(Tags.PLAYER) || go.CompareTag(Tags.AI))
                {
                    var networkObject = go.GetComponentInEntity<NetworkObject>();
                    Debug.Assert(networkObject, $"A player or an AI should have a {nameof(NetworkObject)}");
                    RPC_DropItems(networkObject.Id, go.CompareTag(Tags.PLAYER));
                    EndDash(false);
                    hasHitSomeoneThisFrame = true;
                    return;
                }
                EndDash();
            }
        }

        private void LateUpdate()
        {
            hasHitSomeoneThisFrame = false;
        }
    }
}