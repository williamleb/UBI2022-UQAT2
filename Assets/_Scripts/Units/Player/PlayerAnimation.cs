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
        private static readonly int PlayerWalkSpeed = Animator.StringToHash("playerWalkSpeed");
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

        [Networked] private NetworkBool IsGettingUpF { get; set; } = false;
        [Networked] private NetworkBool IsGettingUpB { get; set; } = false;
        [Networked] private NetworkBool IsDancing { get; set; } = false;
        [Networked] private NetworkBool IsPushing { get; set; } = false;
        [Networked] private NetworkBool IsGrabbing { get; set; } = false;
        [Networked] private NetworkBool IsGiving { get; set; } = false;

        private void AnimationUpdate()
        {
            networkAnimator.Animator.SetBool(IsPlayerMove, CanMove);
            networkAnimator.Animator.SetFloat(PlayerVelocity, velocity);
            networkAnimator.Animator.SetFloat(PlayerWalkSpeed, 1 + velocity / data.MoveMaximumSpeed);
            networkAnimator.Animator.SetFloat(PlayerRunSpeed, 1 + velocity / data.SprintMaximumSpeed);
            networkAnimator.Animator.SetBool(Dashing, IsDashing);
            networkAnimator.Animator.SetBool(IsHolding, inventory.HasHomework);
            networkAnimator.Animator.SetBool(GetUpF, IsGettingUpF);
            networkAnimator.Animator.SetBool(GetUpB, IsGettingUpB);
            networkAnimator.Animator.SetBool(Aiming, IsAiming);
            networkAnimator.Animator.SetBool(Throwing, IsThrowing);
            networkAnimator.Animator.SetBool(Dancing, IsDancing);
            networkAnimator.Animator.SetBool(Pushing, IsPushing);
            networkAnimator.Animator.SetBool(Grabbing, IsGrabbing);
            networkAnimator.Animator.SetBool(Giving, IsGiving);
        }

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

        public async void PlayPushHomeworkAnim()
        {
            IsPushing = true;
            await Task.Delay(250);
            IsPushing = false;
        }
    }
}