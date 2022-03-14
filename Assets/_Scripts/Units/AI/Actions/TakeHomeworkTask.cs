using System;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace Units.AI.Actions
{
    [Serializable]
    [TaskCategory("AI")]
    [TaskDescription("Take the homework task and assign its homework in a shared variable.")]
    public class TakeHomeworkTask : AIAction
    {
        [SerializeField] private SharedTransform targetHomeworkTask;

        public override void OnStart()
        {
            base.OnStart();
            
            if (Brain.TaskSensor)
                Brain.TaskSensor.CanReceiveTask = false;
        }

        public override TaskStatus OnUpdate()
        {
            if (!Brain.TaskSensor)
                return TaskStatus.Failure;

            if (!Brain.TaskSensor.HasHomeworkTask)
                return TaskStatus.Failure;
            
            targetHomeworkTask.SetValue(Brain.TaskSensor.TakeHomeworkTask());
            return TaskStatus.Success;
        }

        public override void OnEnd()
        {
            base.OnEnd();
            
            if (Brain.TaskSensor)
                Brain.TaskSensor.CanReceiveTask = true;
        }

        public override void OnReset()
        {
            base.OnReset();
            targetHomeworkTask = null;
        }
    }
}