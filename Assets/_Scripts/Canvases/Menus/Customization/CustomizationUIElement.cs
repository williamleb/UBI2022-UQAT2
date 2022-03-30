using Units.Customization;
using Units.Player;
using UnityEngine;

namespace Canvases.Menu.Customization
{
    public class CustomizationUIElement : MonoBehaviour
    {
        private PlayerEntity customizablePlayer;

        protected PlayerEntity CustomizablePlayer => customizablePlayer;
        protected PlayerCustomization Customization => customizablePlayer.Customization;

        public void Activate(PlayerEntity player)
        {
            customizablePlayer = player;
            Init();
        }

        public void Deactivate()
        {
            Terminate();
        }

        protected virtual void Init() { }
        protected virtual void Terminate() { }
    }
}