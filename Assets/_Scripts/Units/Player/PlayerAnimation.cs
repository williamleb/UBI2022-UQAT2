using System.Threading.Tasks;
using Fusion;
using UnityEngine;

namespace Units.Player
{
    public partial class PlayerEntity
    {
        [Header("Animation")] [SerializeField] private NetworkMecanimAnimator networkAnimator;
        private static readonly int IsPlayerMove = Animator.StringToHash("isPlayerMove");
        private static readonly int PlayerVelocity = Animator.StringToHash("playerVelocity");
        private static readonly int PlayerRunSpeed = Animator.StringToHash("playerRunSpeed");
        private static readonly int IsHolding = Animator.StringToHash("isHolding");
        private static readonly int Dashing = Animator.StringToHash("isDashing");
        private static readonly int GetUpF = Animator.StringToHash("isGettingUpF");
        private static readonly int GetUpB = Animator.StringToHash("isGettingUpB");
        private static readonly int Aiming = Animator.StringToHash("isAiming");
        private static readonly int Throwing = Animator.StringToHash("isThrowing");
        private static readonly int Dancing = Animator.StringToHash("isDancing");
        private static readonly int Pushing = Animator.StringToHash("isPushing");
        private static readonly int Grabbing = Animator.StringToHash("isGrabbing");
        private static readonly int Giving = Animator.StringToHash("isGiving");

        [Networked(OnChanged = nameof(UpdateGetUpFAnim))]
        private NetworkBool IsGettingUpF { get; set; } = false;

        [Networked(OnChanged = nameof(UpdateGetUpBAnim))] private NetworkBool IsGettingUpB { get; set; } = false;
        [Networked(OnChanged = nameof(UpdateDanceAnim))] private NetworkBool IsDancing { get; set; } = false;
        [Networked(OnChanged = nameof(UpdatePushAnim))] private NetworkBool IsPushing { get; set; } = false;
        [Networked(OnChanged = nameof(UpdateGrabFAnim))] private NetworkBool IsGrabbing { get; set; } = false;
        [Networked(OnChanged = nameof(UpdateGiveFAnim))] private NetworkBool IsGiving { get; set; } = false;

        private void AnimationUpdate()
        {
            networkAnimator.Animator.SetBool(IsPlayerMove, CanMove);
            networkAnimator.Animator.SetFloat(PlayerVelocity, velocity);
            networkAnimator.Animator.SetFloat(PlayerRunSpeed, (1 + velocity) / data.MoveMaximumSpeed);
            networkAnimator.Animator.SetBool(Dashing, IsDashing);
            networkAnimator.Animator.SetBool(IsHolding, inventory.HasHomework);
            networkAnimator.Animator.SetBool(Aiming, IsAiming);
            networkAnimator.Animator.SetBool(Throwing, IsThrowing);
        }

        private static void UpdateGetUpFAnim(Changed<PlayerEntity> changed) =>
            changed.Behaviour.networkAnimator.Animator.SetBool(GetUpF, changed.Behaviour.IsGettingUpF);

        private static void UpdateGetUpBAnim(Changed<PlayerEntity> changed) =>
            changed.Behaviour.networkAnimator.Animator.SetBool(GetUpB, changed.Behaviour.IsGettingUpB);

        private static void UpdateDanceAnim(Changed<PlayerEntity> changed) =>
            changed.Behaviour.networkAnimator.Animator.SetBool(Dancing, changed.Behaviour.IsDancing);

        private static void UpdatePushAnim(Changed<PlayerEntity> changed) =>
            changed.Behaviour.networkAnimator.Animator.SetBool(Pushing, changed.Behaviour.IsPushing);

        private static void UpdateGrabFAnim(Changed<PlayerEntity> changed) =>
            changed.Behaviour.networkAnimator.Animator.SetBool(Grabbing, changed.Behaviour.IsGrabbing);

        private static void UpdateGiveFAnim(Changed<PlayerEntity> changed) =>
            changed.Behaviour.networkAnimator.Animator.SetBool(Giving, changed.Behaviour.IsGiving);

        public async void PlayGrabHomeworkAnim()
        {
            IsGrabbing = true;
            await Task.Delay(500);
            IsGrabbing = false;
        }

        public async void PlayGiveHomeworkAnim()
        {
            IsGiving = true;
            await Task.Delay(500);
            IsGiving = false;
        }

        private async void PlayPushHomeworkAnim()
        {
            IsPushing = true;
            await Task.Delay(250);
            IsPushing = false;
        }

        private async void PlayDancingAnim()
        {
            IsDancing = true;
            await Task.Delay(500);
            IsDancing = false;
        }
    }
}