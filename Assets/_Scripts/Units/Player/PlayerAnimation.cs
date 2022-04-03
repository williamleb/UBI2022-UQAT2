using System.Collections;
using Fusion;
using UnityEngine;
using Utilities;

namespace Units.Player
{
    public partial class PlayerEntity
    {
        [Header("Animation")] [SerializeField] private NetworkMecanimAnimator networkAnimator;

        private static readonly int PlayerVelocity = Animator.StringToHash("playerVelocity");
        private static readonly int PlayerRunSpeed = Animator.StringToHash("playerRunSpeed");
        private static readonly int IsHolding = Animator.StringToHash("isHolding");
        private static readonly int Dashing = Animator.StringToHash("isDashing");
        private static readonly int GetUpFAnim = Animator.StringToHash("IsGettingUpF");
        private static readonly int GetUpBAnim = Animator.StringToHash("IsGettingUpB");
        private static readonly int Throwing = Animator.StringToHash("Throw");
        private static readonly int Aiming = Animator.StringToHash("Aim");
        private static readonly int Giving = Animator.StringToHash("Give");
        private static readonly int Grabbing = Animator.StringToHash("Grab");
        private static readonly int Pushing = Animator.StringToHash("Push");
        private static readonly int Dancing = Animator.StringToHash("Dance");

        private void InitAnim() => inventory.OnInventoryChanged += OnInventoryChangedCallBack;

        private void AnimOnDestroy() => inventory.OnInventoryChanged -= OnInventoryChangedCallBack;

        private void OnInventoryChangedCallBack() => AnimationSetBool(IsHolding, inventory.HasHomework);
        private void OnThrowChanged(bool val) => AnimationSetTrigger(Throwing, val);
        private void OnAimChanged(bool val) => AnimationSetTrigger(Aiming, val);

        [Networked(OnChanged = nameof(OnDancingChangedCallback))]
        private bool IsDancing { get; set; }

        private static void OnDancingChangedCallback(Changed<PlayerEntity> changed) =>
            changed.Behaviour.AnimationSetTrigger(Dancing, changed.Behaviour.IsDancing);

        [Networked(OnChanged = nameof(OnGivingChangedCallback))]
        private bool IsGiving { get; set; }

        private static void OnGivingChangedCallback(Changed<PlayerEntity> changed) =>
            changed.Behaviour.AnimationSetTrigger(Giving, changed.Behaviour.IsGiving);

        [Networked(OnChanged = nameof(OnGrabbingChangedCallback))]
        private bool IsGrabbing { get; set; }

        private static void OnGrabbingChangedCallback(Changed<PlayerEntity> changed) =>
            changed.Behaviour.AnimationSetTrigger(Grabbing, changed.Behaviour.IsGrabbing);

        private void UpdateMoveAnim()
        {
            networkAnimator.Animator.SetFloat(PlayerVelocity, CurrentSpeed);
            networkAnimator.Animator.SetFloat(PlayerRunSpeed, SpeedOnMaxSpeed);
        }

        private void AnimationUpdate()
        {
            if (!isImmune && !networkAnimator.Animator.enabled)
                networkAnimator.Animator.enabled = true;
        }

        private void AnimationSetTrigger(int triggerHash, bool val = true)
        {
            if (!val) return;

            if (Object.HasStateAuthority)
                networkAnimator.SetTrigger(triggerHash);
            else if (Object.HasInputAuthority)
                networkAnimator.Animator.SetTrigger(triggerHash);
        }

        private void AnimationSetBool(int triggerHash, bool val) => networkAnimator.Animator.SetBool(triggerHash, val);

        public void SetGiving()
        {
            StartCoroutine(ResetGiving());
        }

        private IEnumerator ResetGiving()
        {
            IsGiving = true;
            yield return Helpers.GetWait(0.1f);
            IsGiving = false;
        }

        public void SetGrabbing()
        {
            StartCoroutine(ResetGrabbing());
        }

        private IEnumerator ResetGrabbing()
        {
            IsGrabbing = true;
            yield return Helpers.GetWait(0.1f);
            IsGrabbing = false;
        }
    }
}