using BehaviorDesigner.Runtime.Tasks;
using Managers.Interactions;
using UnityEngine;

namespace Units.AI.Actions
{
    [TaskCategory("AIBrain")]
    [TaskDescription("Make the AI walk towards a random interaction.")]
    public class WalkTowardsInteraction : Action
    {
        [SerializeField] private AIBrain brain = null;

        private Interaction interactionToWalkToward;

        public override void OnStart()
        {
            SetNewDestination();
        }

        public override TaskStatus OnUpdate()
        {
            if (!interactionToWalkToward)
                return TaskStatus.Failure;

            if (!interactionToWalkToward.InteractionEnabled)
                return TaskStatus.Failure;

            if (brain.Interacter.CanInteractWith(interactionToWalkToward.InteractionId))
                return TaskStatus.Success;

            brain.SetDestination(interactionToWalkToward.transform.position);
            return TaskStatus.Running;
        }
        
        private void SetNewDestination()
        {
            if (!brain)
                return;

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
            
            if (!interactionToWalkToward)
                return;

            brain.SetDestination(interactionToWalkToward.transform.position);
        }

        public override void OnEnd()
        {
            interactionToWalkToward = null;
        }

        protected virtual bool FilterInteraction(Interaction interaction) => interaction;

        public override void OnReset()
        {
            brain = null;
        }
    }
}