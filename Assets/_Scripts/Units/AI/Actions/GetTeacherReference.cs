using System;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using Action = BehaviorDesigner.Runtime.Tasks.Action;

namespace Units.AI.Actions
{
    [Serializable]
    [TaskCategory("AIManager")]
    public class GetTeacherReference : Action
    {
        [SerializeField] private SharedTransform targetTeacher;

        public override TaskStatus OnUpdate()
        {
            if (!AIManager.HasInstance)
                return TaskStatus.Failure;

            var teacher = AIManager.Instance.Teacher;
            if (!teacher)
                return TaskStatus.Failure;
            
            targetTeacher?.SetValue(teacher.transform);
            return TaskStatus.Success;
        }
    }
}