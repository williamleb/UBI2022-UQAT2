using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace Units.AI.Actions
{
    [TaskCategory("AIBrain")]
    [TaskDescription("Choose a random forward destination for the AI.")]
    public class WalkForwardRandomly : Action
    {
        [BehaviorDesigner.Runtime.Tasks.Tooltip("The play mode of the animation")]
        [SerializeField] private AIBrain brain;
        [BehaviorDesigner.Runtime.Tasks.Tooltip("The maximum angle that the AI can turn")]
        [SerializeField] private float maxAngleOfRotation;
        [BehaviorDesigner.Runtime.Tasks.Tooltip("The distance the AI has to walk before this action is considered complete")]
        [SerializeField] private float distanceToWalk;

        private bool hasSetDestination = false;

        public override void OnStart()
        {
            if (!hasSetDestination)
                SetNewDestination();
        }

        public override TaskStatus OnUpdate()
        {
            if (brain == null) {
                Debug.LogWarning($"{nameof(brain)} is null");
                return TaskStatus.Failure;
            }

            return brain.IsStopped ? TaskStatus.Success : TaskStatus.Failure;
        }
        
        public override void OnReset()
        {
            hasSetDestination = false;
        }

        private void SetNewDestination()
        {
            if (!brain)
                return;

            var angleRotation = Random.Range(0f, maxAngleOfRotation);
            var direction = Quaternion.Euler(0f, angleRotation, 0f) * brain.transform.forward;
            var newDestination = transform.position + direction * distanceToWalk;
            brain.SetDestination(newDestination);
            hasSetDestination = true;
        }
    }
}