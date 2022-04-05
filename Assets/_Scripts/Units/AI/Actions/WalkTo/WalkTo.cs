using System;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace Units.AI.Actions
{
    [Serializable]
    [TaskCategory("AI/Walk To")]
    public abstract class WalkTo : AIAction
    {
        [SerializeField] private SharedBool overrideSpeed = false;        
        [SerializeField] private SharedFloat speed = 0f;        
        [SerializeField] private SharedFloat destinationUpdateRate = 0.2f;        
        
        private float updateCountdown;
        
        protected abstract Vector3 Destination { get; }
        protected virtual bool EndsOnDestinationReached => true;
        protected virtual bool SetDestinationOnStart => true;
        protected virtual bool UpdateDestination => false;
        
        protected virtual void OnBeforeStart() {}
        protected virtual TaskStatus OnUpdateImplementation() => TaskStatus.Running;

        public override void OnStart()
        {
            base.OnStart();
            OnBeforeStart();
            
            if (overrideSpeed.Value)
                Brain.SetSpeed(speed.Value);
            
            if (SetDestinationOnStart)
                SetDestination();
        }

        protected void ForceUpdateDestination()
        {
            SetDestination();
        }

        private void SetDestination()
        {
            Brain.SetDestination(Destination);
        }

        public override TaskStatus OnUpdate()
        {
            ManagePathIncompleteOrInvalid();
            ManageUpdateDestination();

            var implementationUpdateResult = OnUpdateImplementation();
            if (implementationUpdateResult != TaskStatus.Running)
                return implementationUpdateResult;

            if (EndsOnDestinationReached)
                return Brain.HasReachedItsDestination ? TaskStatus.Success : TaskStatus.Running;

            return TaskStatus.Running;
        }

        private void ManagePathIncompleteOrInvalid()
        {
            if (!Brain.IsPathValid())
            {
                OnPathInvalidDetected();
            }
        }
        
        protected virtual void OnPathInvalidDetected() {}

        private void ManageUpdateDestination()
        {
            if (UpdateDestination)
            {
                updateCountdown += Time.deltaTime;
                if (updateCountdown > destinationUpdateRate.Value)
                {
                    SetDestination();
                    updateCountdown = 0f;
                }
            }
        }

        public override void OnEnd()
        {
            base.OnEnd();
            
            if (overrideSpeed.Value)
                Brain.ResetSpeed();
        }

        public override void OnReset()
        {
            base.OnReset();
            overrideSpeed = false;
            speed = 1f;
        }
    }
}