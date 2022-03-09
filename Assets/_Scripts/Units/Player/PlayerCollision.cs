using System.Threading.Tasks;
using UnityEngine;

namespace Units.Player
{
    public partial class PlayerEntity
    {
        private async void Hit()
        {
            CanMove = false;
            await Task.Delay((int)(currentMaxMoveSpeed / data.MoveMaximumSpeed * data.KnockOutTimeInMS));
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