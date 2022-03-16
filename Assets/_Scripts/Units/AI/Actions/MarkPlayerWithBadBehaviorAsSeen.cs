using BehaviorDesigner.Runtime.Tasks;

namespace Units.AI.Actions
{
    [TaskCategory("AI")]
    public class MarkPlayerWithBadBehaviorAsSeen : AIAction
    {
        public override TaskStatus OnUpdate()
        {
            if (!Brain.PlayerBadBehaviorDetection)
                return TaskStatus.Failure;
                
            Brain.PlayerBadBehaviorDetection.MarkBadBehaviorAsSeen();
            Brain.PlayerBadBehaviorDetection.DeactivateUntilNextPoll();
            return TaskStatus.Success;
        }
    }
}