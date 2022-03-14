using BehaviorDesigner.Runtime.Tasks;
using Managers.Interactions;
using UnityEngine;

namespace Units.AI.Actions
{
    [TaskCategory("AI/Walk To")]
    [TaskDescription("Make the AI walk towards a random interaction.")]
    public class WalkToInteraction : WalkTo
    {
        private Interaction interactionToWalkTo;

        protected override Vector3 Destination => interactionToWalkTo.transform.position;
        protected override bool EndsOnDestinationReached => false;
        protected override bool UpdateDestination => interactionToWalkTo != null;
        protected override bool SetDestinationOnStart => interactionToWalkTo != null;
        
        protected virtual bool FilterInteraction(Interaction interaction) => interaction;

        protected override void OnBeforeStart()
        {
            base.OnBeforeStart();
            SetNewDestination();

            if (Brain.TaskSensor)
                Brain.TaskSensor.CanReceiveTask = false;
        }

        protected override TaskStatus OnUpdateImplementation()
        {
            if (!interactionToWalkTo)
                return TaskStatus.Failure;

            if (!interactionToWalkTo.InteractionEnabled)
                return TaskStatus.Failure;

            if (Brain.Interacter.CanInteractWith(interactionToWalkTo.InteractionId))
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
                    interactionToWalkTo = interaction;
                    break;
                }
            }
        }

        public override void OnEnd()
        {
            base.OnEnd();
            interactionToWalkTo = null;

            if (Brain.TaskSensor)
                Brain.TaskSensor.CanReceiveTask = true;
        }
    }
}