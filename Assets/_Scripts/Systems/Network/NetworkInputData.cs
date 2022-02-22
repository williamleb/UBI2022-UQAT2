using Fusion;
using UnityEngine;

namespace Systems.Network
{
    public struct NetworkInputData : INetworkInput
    {
        public const uint BUTTON_DASH = 1 << 0;
        public const uint BUTTON_SPRINT = 1 << 1;
        public const uint BUTTON_INTERACT = 1 << 2;
        public const uint BUTTON_INTERACT_ONCE = 1 << 3;
        public const uint BUTTON_UP = 1 << 4;
        public const uint BUTTON_DOWN = 1 << 5;
        public const uint BUTTON_LEFT = 1 << 6;
        public const uint BUTTON_RIGHT = 1 << 7;

        public uint Buttons;

        private bool IsPressed(uint button) => (Buttons & button) == button;
        
        public bool IsDash => IsPressed(BUTTON_DASH);
        public bool IsUp => IsPressed(BUTTON_UP);
        public bool IsDown => IsPressed(BUTTON_DOWN);
        public bool IsLeft => IsPressed(BUTTON_LEFT);
        public bool IsRight => IsPressed(BUTTON_RIGHT);
        public bool IsSprint => IsPressed(BUTTON_SPRINT);
        public bool IsInteract => IsPressed(BUTTON_INTERACT);
        public bool IsInteractOnce => IsPressed(BUTTON_INTERACT_ONCE);
    }
}