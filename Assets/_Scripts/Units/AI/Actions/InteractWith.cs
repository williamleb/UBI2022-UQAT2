using BehaviorDesigner.Runtime.Tasks;
using Managers.Interactions;
using UnityEngine;

namespace Units.AI.Actions
{
    [TaskCategory("AIBrain")]
    [TaskDescription("Interact with the first interaction in this AI's interaction list.")]
    public class InteractWith : Action
    {
        [SerializeField] private AIBrain brain = null;

        private Interaction interactionToInteractWith;

        public override void OnStart()
        {
            SetNewDestination();
        }

        public override TaskStatus OnUpdate()
        {
            if (!interactionToInteractWith)
                return TaskStatus.Failure;

            if (!interactionToInteractWith.InteractionEnabled)
                return TaskStatus.Failure;

            // TODO Support holding interactions here when it will become a feature
            brain.Interacter.InteractWith(interactionToInteractWith.InteractionId);
            return TaskStatus.Success;
        }
        
        private void SetNewDestination()
        {
            if (!brain)
                return;

            foreach (var interaction in brain.Interacter.InteractionsInReach)
            {
                if (interaction.InteractionEnabled && FilterInteraction(interaction))
                {
                    interactionToInteractWith = interaction;
                    break;
                }
            }
            
            if (!interactionToInteractWith)
                return;

            brain.SetDestination(interactionToInteractWith.transform.position);
        }

        public override void OnEnd()
        {
            interactionToInteractWith = null;
        }

        protected virtual bool FilterInteraction(Interaction interaction) => interaction;

        public override void OnReset()
        {
            brain = null;
        }
    }
}