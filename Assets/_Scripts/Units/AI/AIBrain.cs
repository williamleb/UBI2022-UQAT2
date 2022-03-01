using System;
using BehaviorDesigner.Runtime;
using UnityEngine;

namespace Units.AI
{
    [RequireComponent(typeof(BehaviorTree))]
    public class AIBrain : MonoBehaviour
    {
        private AIEntity entity;
        private BehaviorTree tree;

        public Inventory Inventory => entity.Inventory;
        public AIInteracter Interacter => entity.Interacter;
        
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

        public void StopMoving()
        {
            entity.Agent.SetDestination(entity.transform.position);
            entity.Agent.isStopped = true;
        }

        public void PlayAnimation(string animationName)
        {
            if (!entity.NetworkAnimator)
                return;
            
            entity.NetworkAnimator.SetTrigger(animationName);
        }

        public bool IsPlayingAnimation(int layerIndex, string animationName)
        {
            if (!entity.Animator)
                return false;
            
            return entity.Animator.GetCurrentAnimatorStateInfo(layerIndex).IsName(animationName);
        }

        public bool IsInAnimationTransition(int layerIndex)
        {
            return entity.Animator.IsInTransition(layerIndex);
        }
    }
}