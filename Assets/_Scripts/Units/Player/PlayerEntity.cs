using System;
using Fusion;
using Scriptables;
using Sirenix.OdinInspector;
using Systems;
using Systems.Network;
using Units.AI;
using UnityEngine;

namespace Units.Player
{
    [RequireComponent(typeof(PlayerInteracter))]
    [RequireComponent(typeof(PlayerInputs))]
    [RequireComponent(typeof(Inventory))]
    [ValidateInput(nameof(ValidateIfHasTag), "A PlayerEntity component must be placed on a collider that has the 'Player' tag.")]
    public partial class PlayerEntity : NetworkBehaviour
    {
        public const string TAG = "Player";
        
        private PlayerSettings data;
        private PlayerInteracter interacter;
        private PlayerInputs playerInputs;

        private void Awake()
        {
            data = SettingsSystem.Instance.PlayerSetting;
            
            interacter = GetComponent<PlayerInteracter>();
            playerInputs = GetComponent<PlayerInputs>();
            
            MovementAwake();
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

        public override void FixedUpdateNetwork()
        {
            if (GetInput(out NetworkInputData inputData))
            {
                MoveUpdate(inputData);
                
                if (inputData.Interact)
                {
                    Debug.Log("E");
                    interacter.InteractWithClosestInteraction();
                }
            }
        }
        
        private NetworkInputData GetInput()
        {
            return NetworkInputData.FromPlayerInputs(playerInputs);
        }

        private void OnCollisionEnter(Collision collision) // TODO Replace with the dive feature
        {
            if (collision.gameObject.CompareTag(TAG) || collision.gameObject.CompareTag(AIEntity.TAG))
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
        
        private bool ValidateIfHasTag()
        {
            return gameObject.CompareTag(TAG);
        }
    }
}