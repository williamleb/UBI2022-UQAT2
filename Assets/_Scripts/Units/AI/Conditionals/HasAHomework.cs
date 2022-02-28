using BehaviorDesigner.Runtime.Tasks;

namespace Units.AI.Conditionals
{
    [TaskDescription("Returns success when the inventory contains a homework.")]
    [TaskCategory("AI")]
    public class HasAHomework : AIConditional
    {
        public override TaskStatus OnUpdate()
        {
            return Brain.Inventory.HasHomework ? TaskStatus.Success : TaskStatus.Failure;
        }
    }
}