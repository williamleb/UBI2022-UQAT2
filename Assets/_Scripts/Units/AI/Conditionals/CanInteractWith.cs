using Managers.Interactions;
using Units.AI;
using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks
{
    [TaskDescription("Returns success when the AI can interact with something")]
    [TaskCategory("AIBrain")]
    public class CanInteractWith : Conditional
    {
        [Tooltip("The brain which will use to know if we can interact with something")]
        [SerializeField] private AIBrain brain = null;

        public override TaskStatus OnUpdate()
        {
            if (!brain)
                return TaskStatus.Failure;

            foreach (var interaction in brain.Interacter.InteractionsInReach)
            {
                if (FilterInteraction(interaction))
                    return TaskStatus.Success;
            }
            
            return TaskStatus.Failure;
        }
        
        protected virtual bool FilterInteraction(Interaction interaction) => true;

        public override void OnReset()
        {
            brain = null;
        }
    }
}