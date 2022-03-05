using System;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace Units.AI.Conditionals
{
    [Serializable]
    [TaskDescription("Returns success if the AI has seen a player hitter this frame.")]
    [TaskCategory("AI")]
    public class HasSeenPlayerHitterThisFrame : AIConditional
    {
        [SerializeField] private SharedTransform outPlayerHitterTransform = null;

        public override TaskStatus OnUpdate()
        {
            if (!Brain.PlayerHitterDetection)
                return TaskStatus.Failure;

            var hasSeenPlayerHitter = Brain.PlayerHitterDetection.HasAPlayerHitSomeoneThisFrame;
            if (hasSeenPlayerHitter)
            {
                outPlayerHitterTransform.SetValue(Brain.PlayerHitterDetection.PlayerThatHitSomeoneThisFrame.transform);
            }

            return hasSeenPlayerHitter ? TaskStatus.Success : TaskStatus.Failure;
        }

        public override void OnReset()
        {
            base.OnReset();
            outPlayerHitterTransform = null;
        }
    }
}