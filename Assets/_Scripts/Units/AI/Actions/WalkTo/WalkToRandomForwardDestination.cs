using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace Units.AI.Actions
{
    [TaskCategory("AI/Walk To")]
    [TaskDescription("Choose a random forward destination for the AI.")]
    public class WalkToRandomForwardDestination : WalkTo
    {
        [BehaviorDesigner.Runtime.Tasks.Tooltip("The maximum angle that the AI can turn")]
        [SerializeField] private float maxAngleOfRotation = 45f;
        [BehaviorDesigner.Runtime.Tasks.Tooltip("The distance the AI has to walk before this action is considered complete")]
        [SerializeField] private float distanceToWalk = 2f;

        private Vector3 destination;

        protected override Vector3 Destination => destination;

        protected override void OnBeforeStart()
        {
            base.OnBeforeStart();
            SetNewDestination();
        }

        private void SetNewDestination()
        {
            var angleRotation = Random.Range(0f, maxAngleOfRotation);
            var direction = (Quaternion.Euler(0f, angleRotation, 0f) * Brain.transform.forward).normalized;
            var newDestination = transform.position + direction * distanceToWalk;
            destination = newDestination;
        }

        public override void OnEnd()
        {
            destination = Vector3.zero; 
            base.OnEnd();
        }

        public override void OnReset()
        {
            base.OnReset();
            
            maxAngleOfRotation = 45f;
            distanceToWalk = 2f;
        }
    }
}