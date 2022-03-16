using System;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace Units.AI.Conditionals
{
    [Serializable]
    [TaskDescription("Returns success if the AI has seen a player with bad behavior.")]
    [TaskCategory("AI")]
    public class HasSeenPlayerWithBadBehavior : AIConditional
    {
        [SerializeField] private SharedTransform outPlayerHitterTransform = null;
        [SerializeField] private SharedBool markAsSeen = true;

        public override TaskStatus OnUpdate()
        {
            if (!Brain.PlayerBadBehaviorDetection)
                return TaskStatus.Failure;

            var hasSeenPlayerWithBadBehavior = Brain.PlayerBadBehaviorDetection.HasSeenPlayerWithBadBehavior;
            if (hasSeenPlayerWithBadBehavior)
            {
                outPlayerHitterTransform.SetValue(Brain.PlayerBadBehaviorDetection.PlayerThatHadBadBehavior.transform);
                
                if (markAsSeen.Value)
                    Brain.PlayerBadBehaviorDetection.MarkBadBehaviorAsSeen();
            }

            return hasSeenPlayerWithBadBehavior ? TaskStatus.Success : TaskStatus.Failure;
        }

        public override void OnReset()
        {
            base.OnReset();
            outPlayerHitterTransform = null;
            markAsSeen = true;
        }
    }
}