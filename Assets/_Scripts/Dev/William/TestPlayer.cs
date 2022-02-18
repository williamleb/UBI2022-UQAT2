using System;
using Fusion;
using Systems.Network;
using Units;
using Units.AI;
using Units.Player;
using UnityEngine;
using Utilities.Extensions;

namespace Dev.William
{
    [RequireComponent(typeof(PlayerInteracter))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
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
                var speed = inputData.Sprint ? 10f : 5f;
                characterController.Move(speed * inputData.Move.V2ToFlatV3() * NetworkSystem.DeltaTime);
                
                if (inputData.Interact)
                {
                    interacter.InteractWithClosestInteraction();
                }
            }
        }
        
        private void OnCollisionEnter(Collision collision) // TODO Replace with the dive feature
        {
            if (!Object.HasInputAuthority)
                return;
            
            if (collision.gameObject.CompareTag(PlayerEntity.TAG) || collision.gameObject.CompareTag(AIEntity.TAG))
            {
                var networkObject = collision.gameObject.GetComponent<NetworkObject>();
                Debug.Assert(networkObject, $"A player or an AI should have a {nameof(NetworkObject)}");
                DropItems(networkObject.Id);
            }
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void DropItems(NetworkId entityNetworkId)
        {
            var networkObject = NetworkSystem.Instance.FindObject(entityNetworkId);
            var inventory = networkObject.GetComponent<Inventory>();
            Debug.Assert(inventory, $"A player or an AI should have an {nameof(Inventory)}");
            inventory.DropEverything();
        }
    }
}