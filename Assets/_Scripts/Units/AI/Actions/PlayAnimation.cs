using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace Units.AI.Actions
{
    [TaskCategory("AI")]
    [TaskDescription("Plays an animation that is activated with a trigger.")]
    public class PlayAnimation : AIAction
    {
        private const int NUMBER_OF_TRIES_TO_CONSIDER_ANIMATIONS_STARTED = 20;
        
        [BehaviorDesigner.Runtime.Tasks.Tooltip("Name of the animation to play.")]
        [SerializeField] private SharedString animationName = "";
        [BehaviorDesigner.Runtime.Tasks.Tooltip("Layer index of the animation to play in the animator.")]
        [SerializeField] private SharedInt animationLayerIndex = 0;
        [BehaviorDesigner.Runtime.Tasks.Tooltip("Wait until animation is finished to return success?")]
        [SerializeField] private SharedBool waitUntilAnimationIsFinished = true;

        private bool animationIsStarted = false;
        private int numberOfTriesAskingForStarted = 0;
        
        public override void OnStart()
        {
            base.OnStart();
            animationIsStarted = false;
            numberOfTriesAskingForStarted = 0;
            
            StartAnimation();
        }

        private void StartAnimation()
        {
            Brain.PlayAnimation(animationName.Value);
        }

        public override TaskStatus OnUpdate()
        {
            if (!waitUntilAnimationIsFinished.Value)
                return TaskStatus.Success;

            if (Brain.IsInAnimationTransition(animationLayerIndex.Value))
                return TaskStatus.Running;
            
            var animationIsPlaying = Brain.IsPlayingAnimation(animationLayerIndex.Value, animationName.Value);
            if (!animationIsStarted && animationIsPlaying)
            {
                animationIsStarted = true;
            }
            else if (!animationIsStarted)
            {
                ++numberOfTriesAskingForStarted;
            }
            else if (animationIsStarted && !animationIsPlaying)
            {
                return TaskStatus.Success;
            }

            if (numberOfTriesAskingForStarted > NUMBER_OF_TRIES_TO_CONSIDER_ANIMATIONS_STARTED)
            {
                return TaskStatus.Failure;
            }

            return TaskStatus.Running;
        }

        public override void OnReset()
        {
            base.OnReset();
            animationName = "";
        }
    }
}