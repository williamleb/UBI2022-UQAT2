using Systems.Network;
using Managers.Interactions;

namespace Units.Player
{
    public class PlayerInteracter : Interacter
    {
        public override void FixedUpdateNetwork()
        {
            UpdatePossibleInteraction();
        }

        private void UpdatePossibleInteraction()
        {
            if (!NetworkSystem.Instance.IsConnected || !NetworkSystem.Instance.IsPlayer(Object.InputAuthority))
                return;
            
            if (!InteractionManager.HasInstance)
                return;

            var nearestInteraction = GetClosestAvailableInteraction();
            if (!nearestInteraction)
            {
                InteractionManager.Instance.SetNoInteractionAsPossible();
                return;
            }
            
            InteractionManager.Instance.SetInteractionAsPossible(nearestInteraction.InteractionId);
        }
    }
}