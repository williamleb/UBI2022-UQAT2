namespace Units.AI.Conditionals
{
    using System;
    using BehaviorDesigner.Runtime;
    using BehaviorDesigner.Runtime.Tasks;
    using UnityEngine;

    namespace Units.AI.Conditionals
    {
        [Serializable]
        [TaskDescription("Returns success if the AI has seen a player hitter this frame.")]
        [TaskCategory("AI")]
        public class HasBeenGivenHomeworkThisFrame : AIConditional
        {
            [SerializeField] private SharedTransform outEntityThatGaveOutHomeworkThisFrame = null;

            public override TaskStatus OnUpdate()
            {
                if (!Brain.HomeworkHandingStation)
                    return TaskStatus.Failure;

                var hasEntityGivenHomework = Brain.HomeworkHandingStation.HasAnEntityGivenHomeworkThisFrame;
                if (hasEntityGivenHomework)
                {
                    outEntityThatGaveOutHomeworkThisFrame.SetValue(Brain.HomeworkHandingStation.EntityThatHasGivenHomeworkThisFrame.transform);
                }

                return hasEntityGivenHomework ? TaskStatus.Success : TaskStatus.Failure;
            }

            public override void OnReset()
            {
                base.OnReset();
                outEntityThatGaveOutHomeworkThisFrame = null;
            }
        }
    }
}