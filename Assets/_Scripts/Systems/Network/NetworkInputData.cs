using Fusion;
using UnityEngine;

namespace Systems.Network
{
    public struct NetworkInputData : INetworkInput
    {
        public const uint BUTTON_JUMP = 1 << 0;
        public const uint BUTTON_ATTACK = 1 << 1;
        public const uint BUTTON_ALT_ATTACK = 1 << 2;
        public const uint BUTTON_DASH = 1 << 3;
        public const uint BUTTON_SPRINT = 1 << 4;
        public const uint BUTTON_INTERACT = 1 << 5;

        public uint Buttons;
        public Vector2 Move;
        public Vector2 Look;

        private bool IsDown(uint button) => (Buttons & button) == button;

        public bool IsJump => IsDown(BUTTON_JUMP);
        public bool IsAttack => IsDown(BUTTON_ATTACK);
        public bool IsAltAttack => IsDown(BUTTON_ALT_ATTACK);
        public bool IsDash => IsDown(BUTTON_DASH);
        public bool IsSprint => IsDown(BUTTON_SPRINT);
        public bool IsInteract => IsDown(BUTTON_INTERACT);
    }
}