using BehaviorDesigner.Runtime.Tasks;
using Managers.Interactions;
using UnityEngine;

namespace Units.AI.Actions
{
    [TaskCategory("AI/Walk To")]
    [TaskDescription("Make the AI walk towards a random interaction.")]
    public class WalkToInteraction : WalkTo
    {
        private Interaction interactionToWalkToward;

        protected override Vector3 Destination => interactionToWalkToward.transform.position;
        protected override bool EndsOnDestinationReached => false;
        protected override bool UpdateDestination => interactionToWalkToward;
        protected override bool SetDestinationOnStart => interactionToWalkToward;
        
        protected virtual bool FilterInteraction(Interaction interaction) => interaction;

        protected override void OnBeforeStart()
        {
            base.OnBeforeStart();
            SetNewDestination();
        }

        protected override TaskStatus OnUpdateImplementation()
        {
            if (!interactionToWalkToward)
                return TaskStatus.Failure;

            if (!interactionToWalkToward.InteractionEnabled)
                return TaskStatus.Failure;

            if (Brain.Interacter.CanInteractWith(interactionToWalkToward.InteractionId))
                return TaskStatus.Success;

            return TaskStatus.Running;
        }
        
        private void SetNewDestination()
        {
            if (!InteractionManager.HasInstance)
                return;

            foreach (var interaction in InteractionManager.Instance.Interactions)
            {
                if (interaction.InteractionEnabled && FilterInteraction(interaction))
                {
                    interactionToWalkToward = interaction;
                    break;
                }
            }
        }

        public override void OnEnd()
        {
            interactionToWalkToward = null;
            
            base.OnEnd();
        }
    }
}