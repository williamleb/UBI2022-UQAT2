using BehaviorDesigner.Runtime.Tasks;
using Managers.Interactions;

namespace Units.AI.Conditionals
{
    [TaskDescription("Returns success when the AI can interact with something")]
    [TaskCategory("AI/Can Interact With")]
    public class CanInteractWith : AIConditional
    {
        public override TaskStatus OnUpdate()
        {
            foreach (var interaction in Brain.Interacter.InteractionsInReach)
            {
                if (interaction && FilterInteraction(interaction))
                    return TaskStatus.Success;
            }
            
            return TaskStatus.Failure;
        }
        
        protected virtual bool FilterInteraction(Interaction interaction) => true;
    }
}