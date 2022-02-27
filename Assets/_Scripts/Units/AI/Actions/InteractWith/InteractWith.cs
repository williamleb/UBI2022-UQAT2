using BehaviorDesigner.Runtime.Tasks;
using Managers.Interactions;

namespace Units.AI.Actions
{
    [TaskCategory("AI/Interact With")]
    [TaskDescription("Interact with the first interaction in this AI's interaction list.")]
    public class InteractWith : AIAction
    {
        private Interaction interactionToInteractWith;

        public override void OnStart()
        {
            base.OnStart();
            SetNewDestination();
        }

        public override TaskStatus OnUpdate()
        {
            if (!interactionToInteractWith)
                return TaskStatus.Failure;

            if (!interactionToInteractWith.InteractionEnabled)
                return TaskStatus.Failure;

            // TODO Support holding interactions here when it will become a feature
            Brain.Interacter.InteractWith(interactionToInteractWith.InteractionId);
            return TaskStatus.Success;
        }
        
        private void SetNewDestination()
        {
            foreach (var interaction in Brain.Interacter.InteractionsInReach)
            {
                if (interaction.InteractionEnabled && FilterInteraction(interaction))
                {
                    interactionToInteractWith = interaction;
                    break;
                }
            }
        }

        public override void OnEnd()
        {
            interactionToInteractWith = null;
        }

        protected virtual bool FilterInteraction(Interaction interaction) => interaction;
    }
}