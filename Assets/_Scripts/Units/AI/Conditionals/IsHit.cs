using BehaviorDesigner.Runtime.Tasks;

namespace Units.AI.Conditionals
{
    [TaskDescription("Returns success if the AI is currently hit.")]
    [TaskCategory("AI")]
    public class IsHit : AIConditional
    {
        public override TaskStatus OnUpdate()
        {
            return Brain.IsHit ? TaskStatus.Success : TaskStatus.Failure;
        }
    }
}