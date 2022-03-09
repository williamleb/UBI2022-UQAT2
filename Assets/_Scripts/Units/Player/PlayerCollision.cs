using System.Threading.Tasks;
using UnityEngine;

namespace Units.Player
{
    public partial class PlayerEntity
    {
        private async void Hit()
        {
            CanMove = false;
            int delay = (int) (currentMaxMoveSpeed / data.MoveMaximumSpeed * data.KnockOutTimeInMS);
            delay = Mathf.Max(1000, delay);
            await Task.Delay(delay - 1000);
            AnimGetUpTrigger(); //Only plays the animation if fallen
            await Task.Delay(1000);
            CanMove = true;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (IsMovingFast && collision.gameObject.isStatic)
            {
                Hit();
                AnimStumbleTrigger();
            }
        }
    }
}