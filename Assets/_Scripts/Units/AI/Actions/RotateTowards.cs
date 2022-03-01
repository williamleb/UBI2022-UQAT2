using System;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Units.AI.Actions;
using UnityEngine;

namespace Units.AI
{
    [Serializable]
    [TaskCategory("AI")]
    [TaskDescription("Make the AI rotate towards a transform.")]
    public class RotateTowards : AIAction
    {
        [SerializeField] private SharedTransform towards = null;
        [SerializeField] private SharedFloat speed = 1f;
        [SerializeField] private SharedFloat tolerance = 0.1f;

        public override TaskStatus OnUpdate()
        {
            if (!towards.Value)
                return TaskStatus.Failure;

            var position = towards.Value.position;
            Brain.LookAt(position, speed.Value * Time.deltaTime);
            return Brain.IsLookingAt(position, tolerance.Value) ? TaskStatus.Success : TaskStatus.Running;
        }

        public override void OnReset()
        {
            base.OnReset();
            towards = null;
            speed = 1f;
            tolerance = 0.1f;
        }
    }
}