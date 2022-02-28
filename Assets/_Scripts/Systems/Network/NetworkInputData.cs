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
        public const uint BUTTON_MENU = 1 << 4;

        public uint Buttons;
        public Vector2 Move;

        private bool IsPressed(uint button) => (Buttons & button) == button;
        
        public bool IsDash => IsPressed(BUTTON_DASH);
        public bool IsSprint => IsPressed(BUTTON_SPRINT);
        public bool IsInteract => IsPressed(BUTTON_INTERACT);
        public bool IsInteractOnce => IsPressed(BUTTON_INTERACT_ONCE);
        public bool IsMenu => IsPressed(BUTTON_MENU);
    }
}