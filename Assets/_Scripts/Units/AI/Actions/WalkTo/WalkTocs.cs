using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace Units.AI.Actions
{
    [TaskCategory("AI/Walk To")]
    public abstract class WalkTo : AIAction
    {
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
            
            if (SetDestinationOnStart)
                Brain.SetDestination(Destination);
        }

        public override TaskStatus OnUpdate()
        {
            if (UpdateDestination)
                Brain.SetDestination(Destination);

            var implementationUpdateResult = OnUpdateImplementation();
            if (implementationUpdateResult != TaskStatus.Running)
                return implementationUpdateResult;

            if (EndsOnDestinationReached)
                return Brain.HasReachedItsDestination ? TaskStatus.Success : TaskStatus.Running;

            return TaskStatus.Running;
        }
    }
}