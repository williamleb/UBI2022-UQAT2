using Fusion;
using Systems.Network;
using Units;
using Units.AI;
using Units.Player;
using UnityEngine;
using Utilities.Unity;
using Utilities.Extensions;

namespace Dev.William
{
    [RequireComponent(typeof(PlayerInteracter))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(NetworkCharacterController))]
    public class TestPlayer : NetworkBehaviour
    {
        [SerializeField] private NetworkCharacterController characterController;
        
        private PlayerInteracter interacter;
        private Vector3 moveDirection;

        private void Awake()
        {
            interacter = GetComponent<PlayerInteracter>();
            characterController = GetComponent<NetworkCharacterController>();
        }
        
        public override void FixedUpdateNetwork()
        {
            moveDirection = default;
            if (GetInput(out NetworkInputData inputData))
            {
                moveDirection = inputData.Move.V2ToFlatV3();
                
                var speed = inputData.IsSprint ? 10f : 5f;
                characterController.Move(speed * moveDirection * NetworkSystem.DeltaTime);
                
                if (inputData.IsInteract)
                {
                    interacter.InteractWithClosestInteraction();
                }
            }
        }
        
        private void OnCollisionEnter(Collision collision) // TODO Replace with the dive feature
        {
            if (!Object.HasInputAuthority)
                return;
            
            if (collision.gameObject.CompareTag(Tags.PLAYER) || collision.gameObject.CompareTag(Tags.AI))
            {
                var networkObject = collision.gameObject.GetComponent<NetworkObject>();
                Debug.Assert(networkObject, $"A player or an AI should have a {nameof(NetworkObject)}");
                RPC_DropItems(networkObject.Id);
            }
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_DropItems(NetworkId entityNetworkId)
        {
            var networkObject = NetworkSystem.Instance.FindObject(entityNetworkId);
            var inventory = networkObject.GetComponent<Inventory>();
            Debug.Assert(inventory, $"A player or an AI should have an {nameof(Inventory)}");
            inventory.DropEverything();
        }
    }
}