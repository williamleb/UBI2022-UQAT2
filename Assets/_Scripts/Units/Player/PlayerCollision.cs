using System.Threading.Tasks;

namespace Units.Player
{
    public partial class PlayerEntity
    {
        private async void Hit()
        {
            CanMove = false;
            await Task.Delay((int)(currentMoveSpeed / data.MoveMaximumSpeed * data.KnockOutTimeInMS));
            CanMove = true;
        }
    }
}