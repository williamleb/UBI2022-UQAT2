using System.Linq;
using Managers.Interactions;

namespace Units.AI
{
    public class AIInteracter : Interacter
    {
        public bool CanInteractWith(int interactionId)
        {
            return InteractionsInReach.Any(interaction => interaction.InteractionId == interactionId);
        }

        public bool InteractWith(int interactionId)
        {
            var interaction = InteractionsInReach.FirstOrDefault(interaction => interaction.InteractionId == interactionId);
            if (interaction == null)
                return false;

            interaction.Interact(this);
            return true;
        }
    }
}