using System;
using BehaviorDesigner.Runtime;
using Managers.Hallway;
using Units.AI.Senses;
using UnityEngine;

namespace Units.AI
{
    [RequireComponent(typeof(BehaviorTree))]
    public class AIBrain : MonoBehaviour
    {
        private AIEntity entity;
        private BehaviorTree tree;

        public float BaseSpeed => entity.BaseSpeed;
        public Vector3 Position => entity.transform.position;
        public HallwayColor AssignedHallway => entity.AssignedHallway;
        public Inventory Inventory => entity.Inventory;
        public AIInteracter Interacter => entity.Interacter;
        public PlayerBadBehaviorDetection PlayerBadBehaviorDetection => entity.PlayerBadBehaviorDetection;
        public HomeworkHandingStation HomeworkHandingStation => entity.HomeworkHandingStation;
        public AITaskSensor TaskSensor => entity.TaskSensor;
        
        public bool HasReachedItsDestination => !entity.Agent.pathPending &&
                                                entity.Agent.remainingDistance <= entity.Agent.stoppingDistance &&
                                                (!entity.Agent.hasPath || Math.Abs(entity.Agent.velocity.sqrMagnitude) < 0.01f);

        public bool IsHit => entity.IsHit;

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

        public void LookAt(Vector3 destination, float speed)
        {
            var lookPos = destination - entity.transform.position;
            lookPos.y = 0;
            var rotation = Quaternion.LookRotation(lookPos);
            entity.transform.rotation = Quaternion.Slerp(entity.transform.rotation, rotation, speed);  
        }

        public bool IsLookingAt(Vector3 destination, float tolerance)
        {
            var thisTransform = entity.transform;
            var lookPos = destination - thisTransform.position;
            var forward = thisTransform.forward;

            return Math.Abs(Vector3.Dot(lookPos.normalized, forward.normalized) - 1f) < tolerance;
        }

        public bool IsCloseTo(Vector3 destination, float closeDistance)
        {
            var direction = destination - entity.transform.position;

            return direction.sqrMagnitude < closeDistance * closeDistance;
        }

        public void SetSpeed(float speed)
        {
            entity.Agent.speed = speed;
        }

        public void ResetSpeed()
        {
            entity.Agent.speed = BaseSpeed;
        }
    }
}