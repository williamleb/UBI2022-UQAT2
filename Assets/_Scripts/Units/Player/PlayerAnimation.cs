using UnityEngine;

namespace Units.Player
{
    public partial class PlayerEntity
    {
        [SerializeField] private Animator animator;
        private static readonly int IsPlayerMove = Animator.StringToHash("isPlayerMove");
        private static readonly int PlayerSpeed = Animator.StringToHash("playerSpeed");

        private void AnimationUpdate()
        {
            animator.SetBool(IsPlayerMove,IsPlayerMoving);
            animator.SetFloat(PlayerSpeed, CurrentMoveSpeed/data.MoveMaximumSpeed);
        }

        private void AnimDashTrigger()
        {
        }
        
        //Stop animation to allow ragdoll
        
        //Get up animation
        
        //Aim
        
        //Launch
        
        //Pickup
        
        //Hold
    }
}