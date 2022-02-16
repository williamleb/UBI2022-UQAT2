using System;
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
        
        public bool HasReachedItsDestination => !entity.Agent.pathPending &&
                                                entity.Agent.remainingDistance <= entity.Agent.stoppingDistance &&
                                                (!entity.Agent.hasPath || Math.Abs(entity.Agent.velocity.sqrMagnitude) < 0.01f);

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