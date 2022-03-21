using Units.Player;
using UnityEngine;

namespace Canvases.Customization
{
    public class CustomizationUIElement : MonoBehaviour
    {
        private PlayerEntity customizablePlayer;

        protected PlayerEntity CustomizablePlayer;

        public void Activate(PlayerEntity player)
        {
            customizablePlayer = player;
            Init();
        }

        protected virtual void Init() { }
    }
}