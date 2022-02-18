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
        [SerializeField] private NetworkCharacterController characterController;
        
        private PlayerInteracter interacter;

        private void Awake()
        {
            interacter = GetComponent<PlayerInteracter>();
            characterController = GetComponent<NetworkCharacterController>();
        }
        
        public override void FixedUpdateNetwork()
        {
            if (GetInput(out NetworkInputData inputData))
            {
                characterController.Move(5 * inputData.Move.V2ToFlatV3() * NetworkSystem.DeltaTime);
                
                if (inputData.IsInteract)
                {
                    Debug.Log("E");
                    interacter.InteractWithClosestInteraction();
                }
            }
        }
    }
}