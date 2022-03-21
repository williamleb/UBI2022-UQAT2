using Units.Player;
using UnityEngine;

namespace Canvases.Menu.Customization
{
    public class CustomizationUIElement : MonoBehaviour
    {
        private PlayerEntity customizablePlayer;

        protected PlayerEntity CustomizablePlayer => customizablePlayer;

        public void Activate(PlayerEntity player)
        {
            customizablePlayer = player;
            Init();
        }

        protected virtual void Init() { }
    }
}