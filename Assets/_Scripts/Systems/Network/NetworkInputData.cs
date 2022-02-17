using Fusion;
using Units.Player;
using UnityEngine;

namespace Systems.Network
{
    public struct NetworkInputData : INetworkInput
    {
        public Vector2 Move;
        public Vector2 Look;
        public bool Jump;
        public bool Attack;
        public bool AltAttack;
        public bool Dash;
        public bool Sprint;
        public bool Interact;

        public static NetworkInputData FromPlayerInputs(PlayerInputs playerInputs)
        {
            return new NetworkInputData()
            {
              Move = playerInputs.Move,
              Look = playerInputs.Look,
              Jump = playerInputs.Jump,
              Attack = playerInputs.Attack,
              AltAttack = playerInputs.AltAttack,
              Dash = playerInputs.Dash,
              Sprint = playerInputs.Sprint,
              Interact = playerInputs.Interact
            };
        }
        
        public static NetworkInputData FromNoInput()
        {
            return new NetworkInputData()
            {
                Move = Vector2.zero,
                Look = Vector2.zero,
                Jump = false,
                Attack = false,
                AltAttack = false,
                Dash = false,
                Sprint = false,
                Interact = false
            };
        }
    }
}