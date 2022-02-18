using Fusion;
using Systems.Network;
using Units.Player;
using UnityEngine;
using Utilities.Extensions;

namespace Dev.William
{
    [RequireComponent(typeof(PlayerInteracter))]
    [RequireComponent(typeof(NetworkCharacterController))]
    public class TestPlayer : NetworkBehaviour
    {
        [SerializeField] private PlayerInputHandler playerInputHandler;
        [SerializeField] private NetworkCharacterController characterController;
        
        private PlayerInteracter interacter;

        private void Awake()
        {
            interacter = GetComponent<PlayerInteracter>();
            characterController = GetComponent<NetworkCharacterController>();
        }
        
        public override void Spawned()
        {
            if (Object.HasInputAuthority)
            {
                NetworkSystem.Instance.OnInputEvent += GetInput;
            }
        }

        private void GetInput(NetworkRunner runner, NetworkInput input)
        {
            input.Set(NetworkInputData.FromPlayerInputs(playerInputHandler));
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (NetworkSystem.HasInstance && Object.HasInputAuthority)
            {
                NetworkSystem.Instance.OnInputEvent -= GetInput;
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (GetInput(out NetworkInputData inputData))
            {
                characterController.Move(5 * inputData.Move.V2ToFlatV3() * NetworkSystem.DeltaTime);
                
                if (inputData.Interact)
                {
                    Debug.Log("E");
                    interacter.InteractWithClosestInteraction();
                }
            }
        }
    }
}