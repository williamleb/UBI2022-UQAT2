using System.Collections.Generic;
using Utilities.Singleton;

namespace Managers.Interactions
{
    public class InteractionManager : Singleton<InteractionManager>
    {
        private List<Interaction> interactions = new List<Interaction>();

        public void RegisterInteraction(Interaction interaction)
        {
            interactions.Add(interaction);
        }

        public void UnregisterInteraction(Interaction interaction)
        {
            interactions.Remove(interaction);
        }
    }
}