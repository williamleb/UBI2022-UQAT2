using BehaviorDesigner.Runtime;
using UnityEngine;
using UnityEngine.AI;

namespace Units.AI
{
    // ReSharper disable once InconsistentNaming Reason: AI should be uppercase
    [RequireComponent(typeof(BehaviorTree))]
    public class AIBrain : MonoBehaviour
    {
        private AIEntity entity;
        private BehaviorTree tree;
        
        public bool IsStopped => entity.Agent.isStopped;

        public void AssignEntity(AIEntity assignedEntity)
        {
            entity = assignedEntity;
        }

        private void Awake()
        {
            tree = GetComponent<BehaviorTree>();
        }

        public void SetDestination(Vector3 target)
        {
            entity.Agent.SetDestination(target);
        }
    }
}