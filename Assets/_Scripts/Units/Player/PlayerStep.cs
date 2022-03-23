using Sirenix.OdinInspector;
using UnityEngine;
using Utilities.Extensions;

namespace Units.Player
{
    [RequireComponent(typeof(Animator))]
    public class PlayerStep : MonoBehaviour
    {
        [SerializeField, Required] private PlayerEntity playerEntity;
        [SerializeField] private int stepAnimatorLayer;

        private Animator animator;

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        private void AnimationEvent_Step(AnimationEvent animationEvent)
        {
            if (animator.GetLayer(animationEvent.animatorStateInfo) != stepAnimatorLayer)
                return;
            
            playerEntity.PlayFootstepSoundLocally();
        }
    }
}