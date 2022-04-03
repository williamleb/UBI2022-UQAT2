using Fusion;
using UnityEngine;

namespace Units.Player
{
    public partial class PlayerEntity
    {
        [Header("Animation")] [SerializeField] private NetworkMecanimAnimator networkAnimator;
        
        //To update every frame
        private static readonly int PlayerVelocity = Animator.StringToHash("playerVelocity");
        private static readonly int PlayerRunSpeed = Animator.StringToHash("playerRunSpeed");
        private static readonly int IsHolding = Animator.StringToHash("isHolding");
        private static readonly int Dashing = Animator.StringToHash("isDashing");
        private static readonly int GetUpFAnim = Animator.StringToHash("IsGettingUpF");
        private static readonly int GetUpBAnim = Animator.StringToHash("IsGettingUpB");

        //To update on trigger
        private static readonly int Throwing = Animator.StringToHash("Throw");
        private static readonly int Aiming = Animator.StringToHash("Aim");
        public static readonly int Giving = Animator.StringToHash("Give");
        public static readonly int Grabbing = Animator.StringToHash("Grab");
        private static readonly int Pushing = Animator.StringToHash("Push");
        private static readonly int Dancing = Animator.StringToHash("Dance");

        private void InitAnim() => inventory.OnInventoryChanged += OnInventoryChangedCallBack;

        private void AnimOnDestroy() => inventory.OnInventoryChanged -= OnInventoryChangedCallBack;

        private void OnInventoryChangedCallBack() => AnimationSetBool(IsHolding, inventory.HasHomework);
        private void OnThrowChanged(bool val) => AnimationSetTrigger(Throwing, val);
        private void OnAimChanged(bool val) => AnimationSetTrigger(Aiming, val);

        [Networked(OnChanged = nameof(OnDancingChangedCallback))]
        private bool IsDancing { get; set; }
        private static void OnDancingChangedCallback(Changed<PlayerEntity> changed) => changed.Behaviour.AnimationSetTrigger(Dancing, changed.Behaviour.IsDancing);
        
        private void AnimationUpdate()
        {
            base.FixedUpdateNetwork();
            networkAnimator.Animator.SetFloat(PlayerVelocity, CurrentSpeed);
            networkAnimator.Animator.SetFloat(PlayerRunSpeed, SpeedOnMaxSpeed);
        }

        public void AnimationSetTrigger(int triggerHash, bool val = true)
        {
            if (!val) return;
            
            if (Object.HasStateAuthority)
                networkAnimator.SetTrigger(triggerHash);
            else if (Object.HasInputAuthority)
                networkAnimator.Animator.SetTrigger(triggerHash);
        }

        private void AnimationSetBool(int triggerHash, bool val) => networkAnimator.Animator.SetBool(triggerHash, val);
    }
}