using System.Collections.Generic;
using Utilities.Singleton;

namespace Managers.Interactions
{
    public class InteractionManager : Singleton<InteractionManager>
    {
        private readonly Dictionary<int, Interaction> interactions = new Dictionary<int, Interaction>();

        public IEnumerable<Interaction> Interactions => interactions.Values;

        public void RegisterInteraction(Interaction interaction)
        {
            interactions.Add(interaction.InteractionId, interaction);
        }

        public void UnregisterInteraction(Interaction interaction)
        {
            interactions.Remove(interaction.InteractionId);
        }

        public Interaction GetInteraction(int interactionId)
        {
            return interactions.ContainsKey(interactionId) ? interactions[interactionId] : null;
        }

        public void SetInteractionAsPossible(int interactionId)
        {
            foreach (var interaction in interactions.Values)
            {
                interaction.Possible = interaction.InteractionId == interactionId;
            }
        }

        public void SetNoInteractionAsPossible()
        {
            foreach (var interaction in interactions.Values)
            {
                interaction.Possible = false;
            }
        }
    }
}