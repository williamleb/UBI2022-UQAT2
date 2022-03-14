using System;
using BehaviorDesigner.Runtime.Tasks;

namespace Units.AI.Conditionals
{
    [Serializable]
    [TaskDescription("Returns success if the AI has an homework task assigned.")]
    [TaskCategory("AI")]
    public class HasHomeworkTask : AIConditional
    {
        public override TaskStatus OnUpdate()
        {
            if (!Brain.TaskSensor)
                return TaskStatus.Failure;

            return Brain.TaskSensor.HasHomeworkTask ? TaskStatus.Success : TaskStatus.Failure;
        }
    }
}