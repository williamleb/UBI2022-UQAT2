using System;
using BehaviorDesigner.Runtime.Tasks;
using Ingredients.Homework;

namespace Units.AI.Actions
{
    [Serializable]
    [TaskCategory("AI/Walk To")]
    [TaskDescription("Make the AI walk towards a homework.")]
    public class WalkToHomework : WalkToTransform
    {
        private Homework homeworkToWalkTo = null;
        
        public override void OnStart()
        {
            base.OnStart();

            if (Brain.TaskSensor)
                Brain.TaskSensor.CanReceiveTask = false;

            if (TransformToWalkTo.Value)
                homeworkToWalkTo = TransformToWalkTo.Value.GetComponent<Homework>();
        }
        
        protected override TaskStatus OnUpdateImplementation()
        {
            if (!homeworkToWalkTo)
                return TaskStatus.Failure;

            if (!homeworkToWalkTo.IsInWorld)
            {
                TransformToWalkTo.SetValue(null);
                return TaskStatus.Failure;
            }

            return TaskStatus.Running;
        }

        public override void OnEnd()
        {
            base.OnEnd();
            
            if (Brain.TaskSensor)
                Brain.TaskSensor.CanReceiveTask = true;
        }
    }
}