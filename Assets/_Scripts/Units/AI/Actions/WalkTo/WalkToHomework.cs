using System;
using BehaviorDesigner.Runtime.Tasks;

namespace Units.AI.Actions
{
    [Serializable]
    [TaskCategory("AI/Walk To")]
    [TaskDescription("Make the AI walk towards a homework.")]
    public class WalkToHomework : WalkToTransform
    {
        public override void OnStart()
        {
            base.OnStart();

            if (Brain.TaskSensor)
                Brain.TaskSensor.CanReceiveTask = false;
        }

        public override void OnEnd()
        {
            base.OnEnd();
            
            if (Brain.TaskSensor)
                Brain.TaskSensor.CanReceiveTask = false;
        }
    }
}