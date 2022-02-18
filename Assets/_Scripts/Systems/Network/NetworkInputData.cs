using Fusion;
using Units.Player;
using UnityEngine;

namespace Systems.Network
{
    public struct NetworkInputData : INetworkInput
    {
        public Vector2 Move;
        public Vector2 Look;
        public NetworkBool Jump;
        public NetworkBool Attack;
        public NetworkBool AltAttack;
        public NetworkBool Dash;
        public NetworkBool Sprint;
        public NetworkBool Interact;

        public static NetworkInputData FromPlayerInputs(PlayerInputHandler playerInputHandler)
        {
            return new NetworkInputData
            {
              Move = playerInputHandler.Move,
              Look = playerInputHandler.Look,
              Jump = playerInputHandler.Jump,
              Attack = playerInputHandler.Attack,
              AltAttack = playerInputHandler.AltAttack,
              Dash = playerInputHandler.Dash,
              Sprint = playerInputHandler.Sprint,
              Interact = playerInputHandler.Interact
            };
        }
    }
}