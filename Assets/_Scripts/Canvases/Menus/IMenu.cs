using Units.Player;

namespace Canvases.Menu
{
    public interface IMenu
    {
        void ShowFor(PlayerEntity playerEntity);
        void Hide();
    }
}