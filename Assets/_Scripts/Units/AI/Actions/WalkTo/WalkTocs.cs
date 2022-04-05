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
                Brain.SetDestination(Destination);
        }

        protected void ForceUpdateDestination()
        {
            Brain.SetDestination(Destination);
        }

        private void ResetDestination()
        {
            
        }

        private float updateCountdown;
        public override TaskStatus OnUpdate()
        {
            if (UpdateDestination)
            {
                updateCountdown += Time.deltaTime;
                if (updateCountdown > 0.2f)
                {
                    Brain.SetDestination(Destination);
                    updateCountdown = 0f;
                }
            }

            var implementationUpdateResult = OnUpdateImplementation();
            if (implementationUpdateResult != TaskStatus.Running)
                return implementationUpdateResult;

            if (EndsOnDestinationReached)
                return Brain.HasReachedItsDestination ? TaskStatus.Success : TaskStatus.Running;

            return TaskStatus.Running;
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