using Fusion;
using Systems.Network;
using Managers.Interactions;

namespace Units.Player
{
    public class PlayerInteracter : Interacter
    {
        private const int NO_INTERACTION = -1;

        [Networked(OnChanged = nameof(OnNearestInteractionIdChanged), OnChangedTargets = OnChangedTargets.InputAuthority)] private int NearestInteractionId {get;set;}
        
        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();
            UpdateClosestInteractionId();
        }

        private void UpdateClosestInteractionId()
        {
            if (!NetworkSystem.Instance.IsConnected || !NetworkSystem.Instance.IsHost)
                return;
            
            if (!InteractionManager.HasInstance)
                return;

            var nearestInteraction = GetClosestAvailableInteraction();
            if (!nearestInteraction)
            {
                NearestInteractionId = NO_INTERACTION;
                return;
            }

            NearestInteractionId = nearestInteraction.InteractionId;
        }

        private void UpdatePossibleInteraction()
        {
            if (NearestInteractionId == NO_INTERACTION)
            {
                InteractionManager.Instance.SetNoInteractionAsPossible();
                return;
            }
            
            InteractionManager.Instance.SetInteractionAsPossible(NearestInteractionId);
        }
        
        private static void OnNearestInteractionIdChanged(Changed<PlayerInteracter> changed)
        {
            changed.Behaviour.UpdatePossibleInteraction();
        }
    }
}