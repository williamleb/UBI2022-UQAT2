using System;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Ingredients.Homework;
using Units.AI.Actions;
using UnityEngine;

namespace Units.AI
{
    [Serializable]
    [TaskCategory("AI")]
    [TaskDescription("Transfer the AI task if it could not be completed.")]
    public class TransferHomeworkTask : AIAction
    {
        [SerializeField] private SharedTransform targetHomeworkTask;

        public override TaskStatus OnUpdate()
        {
            if (!AITaskOrchestrator.HasInstance)
                return TaskStatus.Failure;

            if (!targetHomeworkTask.Value)
                return TaskStatus.Failure;

            var homeworkTask = targetHomeworkTask.Value.GetComponent<Homework>();
            if (!homeworkTask)
                return TaskStatus.Failure;
            
            targetHomeworkTask.SetValue(null);
            AITaskOrchestrator.Instance.TransferHomeworkTask(homeworkTask);
            return TaskStatus.Success;
        }
    }
}