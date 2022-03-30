using BehaviorDesigner.Runtime.Tasks;

namespace Units.AI.Actions.Alert
{
    [TaskCategory("AI/Alert")]
    public class PlayAlert : AIAction
    {
        public override TaskStatus OnUpdate()
        {
            Brain.PlayAlert();
            return TaskStatus.Success;
        }
    }
}