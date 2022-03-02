using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace Units.AI.Actions
{
    [TaskCategory("AI")]
    [TaskIcon("{SkinColor}WaitIcon.png")]
    public class InterruptibleWait : Wait
    {
        [BehaviorDesigner.Runtime.Tasks.Tooltip("Should the time the task passes when interrupted contribute to the wait time?")]
        [SerializeField] private readonly SharedBool interruptionContributesToWaitTime = true;

        private float endTime;
        private bool hasFinished;

        public override void OnAwake()
        {
            base.OnAwake();
            hasFinished = true;
        }

        public override void OnStart()
        {
            if (hasFinished)
            {
                base.OnStart();
            }
            else if (!interruptionContributesToWaitTime.Value)
            {
                StartTime += Time.time - endTime;
            }
            
            hasFinished = false;
        }

        public override TaskStatus OnUpdate()
        {
            var result = base.OnUpdate();

            if (result != TaskStatus.Running)
                hasFinished = true;

            return result;
        }

        public override void OnEnd()
        {
            base.OnEnd();
            endTime = Time.time;
        }
    }
}