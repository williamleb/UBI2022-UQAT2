using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities.Extensions;

namespace Units.Player
{
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimationEventHandler : MonoBehaviour
    {
        [SerializeField, Required] private PlayerEntity playerEntity;
        [SerializeField] private int stepAnimatorLayer;

        private Animator animator;

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        [UsedImplicitly]
        private void AnimationEvent_Step(AnimationEvent animationEvent)
        {
            if (animator.GetLayer(animationEvent.animatorStateInfo) != stepAnimatorLayer)
                return;

            playerEntity.PlayFootstepSoundLocally();
        }

        [UsedImplicitly]
        private void AnimationEvent_Throw(AnimationEvent animationEvent)
        {
            playerEntity.ThrowOnAnimEvent();
        }

        [UsedImplicitly]
        private void AnimationEvent_GetUp(AnimationEvent animationEvent)
        {
            playerEntity.IsUpAnimEvent();
        }
    }
}