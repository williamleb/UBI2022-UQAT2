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

        private void AnimationUpdate()
        {
            networkAnimator.Animator.SetBool(IsPlayerMove,IsPlayerMoving);
            networkAnimator.Animator.SetFloat(PlayerSpeed, CurrentMoveSpeed/data.MoveMaximumSpeed);
            networkAnimator.Animator.SetBool(Dashing,IsDashing);
            networkAnimator.Animator.SetBool(IsHolding,inventory.HasHomework);
        }

        private void AnimStumbleTrigger() => networkAnimator.SetTrigger("stumble");
        private void AnimFallTrigger() => networkAnimator.SetTrigger("fall");
        private void AnimGetUpTrigger() => networkAnimator.SetTrigger("GetUp");

        //Stop animation to allow ragdoll

        //Aim
        
        //Launch
        
        //Pickup
        
        //Hold
    }
}