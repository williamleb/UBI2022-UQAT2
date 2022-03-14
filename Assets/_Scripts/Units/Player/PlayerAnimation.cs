using Fusion;
using UnityEngine;

namespace Units.Player
{
    public partial class PlayerEntity
    {
        [SerializeField] private NetworkMecanimAnimator networkAnimator;
        private static readonly int IsPlayerMove = Animator.StringToHash("isPlayerMove");
        private static readonly int PlayerSpeed = Animator.StringToHash("playerSpeed");
        private static readonly int Dashing = Animator.StringToHash("isDashing");
        private static readonly int IsHolding = Animator.StringToHash("isHolding");
        private static readonly int GetUpF = Animator.StringToHash("GetUpF");
        private static readonly int GetUpB = Animator.StringToHash("GetUpB");
        private static readonly int Aiming = Animator.StringToHash("isAiming");
        private static readonly int Throwing = Animator.StringToHash("isThrowing");

        [Networked] private NetworkBool IsGettingUpF { get; set; } = false;
        [Networked] private NetworkBool IsGettingUpB { get; set; } = false;

        private void AnimationUpdate()
        {
            networkAnimator.Animator.SetBool(IsPlayerMove, CanMove);
            networkAnimator.Animator.SetFloat(PlayerSpeed, velocity / data.MoveMaximumSpeed);
            networkAnimator.Animator.SetBool(Dashing, IsDashing);
            networkAnimator.Animator.SetBool(IsHolding, inventory.HasHomework);
            networkAnimator.Animator.SetBool(GetUpF,IsGettingUpF);
            networkAnimator.Animator.SetBool(GetUpB,IsGettingUpB);
            networkAnimator.Animator.SetBool(Aiming,IsAiming);
            networkAnimator.Animator.SetBool(Throwing,IsThrowing);
        }

        //Pickup
    }
}