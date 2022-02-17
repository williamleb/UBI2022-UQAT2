using Scriptables;
using Systems;
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
        [SerializeField] private PlayerInputs playerInputs;
        [SerializeField] private NetworkCharacterController characterController;
        
        private PlayerInteracter interacter;

        private void Awake()
        {
            interacter = GetComponent<PlayerInteracter>();
            characterController = GetComponent<NetworkCharacterController>();
        }
        
        public override void Spawned()
        {
            if (NetworkSystem.Instance.IsPlayer(Object.InputAuthority))
            {
                NetworkSystem.Instance.SetInputFunction(GetInput);
            }
        }
        
        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (NetworkSystem.HasInstance && NetworkSystem.Instance.IsPlayer(Object.InputAuthority))
            {
                NetworkSystem.Instance.UnsetInputFunction();
            }
        }
        
        private NetworkInputData GetInput()
        {
            return NetworkInputData.FromPlayerInputs(playerInputs);
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