using System;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace Units.AI.Actions
{
    [Serializable]
    [TaskCategory("AI/Walk To")]
    [TaskDescription("Make the AI walk towards a transform.")]
    public class WalkToTransform : WalkTo
    {
        [SerializeField] private SharedTransform transformToWalkTo = null;
        [SerializeField] private SharedFloat distanceToStop = 1f;
        
        protected override bool EndsOnDestinationReached => false;
        protected override bool SetDestinationOnStart => transformToWalkTo.Value != null;
        protected override bool UpdateDestination => transformToWalkTo.Value != null;

        protected override Vector3 Destination => transformToWalkTo.Value.position;

        protected override TaskStatus OnUpdateImplementation()
        {
            if (!transformToWalkTo.Value)
                return TaskStatus.Failure;
            
            return Brain.IsCloseTo(Destination, distanceToStop.Value) ? TaskStatus.Success : TaskStatus.Running;
        }

        public override void OnReset()
        {
            base.OnReset();
            transformToWalkTo = null;
            distanceToStop = 1f;
        }
    }
}