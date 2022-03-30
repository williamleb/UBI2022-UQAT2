using BehaviorDesigner.Runtime.Tasks;

namespace Units.AI.Actions.Alert
{
    [TaskCategory("AI/Alert")]
    public class StopAlert : AIAction
    {
        public override TaskStatus OnUpdate()
        {
            Brain.StopAlert();
            return TaskStatus.Success;
        }
    }
}