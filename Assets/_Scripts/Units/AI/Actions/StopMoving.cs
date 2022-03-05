using BehaviorDesigner.Runtime.Tasks;

namespace Units.AI.Actions
{
    [TaskCategory("AI")]
    [TaskDescription("Make the AI completely stop moving.")]
    public class StopMoving : AIAction
    {
        public override TaskStatus OnUpdate()
        {
            Brain.StopMoving();
            return TaskStatus.Success;
        }
    }
}