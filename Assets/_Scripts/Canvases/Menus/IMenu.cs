using Units.Player;

namespace Canvases.Menu
{
    public interface IMenu
    {
        bool ShowFor(PlayerEntity playerEntity);
        bool Show();
        bool Hide();
    }
}