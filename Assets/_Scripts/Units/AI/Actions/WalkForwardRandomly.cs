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
        [SerializeField] private readonly AIBrain brain = null;
        [BehaviorDesigner.Runtime.Tasks.Tooltip("The maximum angle that the AI can turn")]
        [SerializeField] private float maxAngleOfRotation = 45f;
        [BehaviorDesigner.Runtime.Tasks.Tooltip("The distance the AI has to walk before this action is considered complete")]
        [SerializeField] private float distanceToWalk = 2f;

        public override void OnStart()
        {
            SetNewDestination();
        }

        public override TaskStatus OnUpdate()
        {
            if (brain == null) 
            {
                Debug.LogWarning($"{nameof(brain)} is null");
                return TaskStatus.Failure;
            }

            return brain.HasReachedItsDestination ? TaskStatus.Success : TaskStatus.Running;
        }
        
        private void SetNewDestination()
        {
            if (!brain)
                return;

            var angleRotation = Random.Range(0f, maxAngleOfRotation);
            var direction = (Quaternion.Euler(0f, angleRotation, 0f) * brain.transform.forward).normalized;
            var newDestination = transform.position + direction * distanceToWalk;
            brain.SetDestination(newDestination);
        }
    }
}