using System;
using Fusion;
using Scriptables;
using Sirenix.OdinInspector;
using Systems;
using Systems.Network;
using Units.Camera;
using Units.AI;
using UnityEngine;
using Utilities.Extensions;

namespace Units.Player
{
    [RequireComponent(typeof(PlayerInteracter))]
    [RequireComponent(typeof(PlayerInputHandler))]
    [RequireComponent(typeof(Inventory))]
    [ValidateInput(nameof(ValidateIfHasTag), "A PlayerEntity component must be placed on a collider that has the 'Player' tag.")]
    public partial class PlayerEntity : NetworkBehaviour
    {
        public const string TAG = "Player";
        
        public static event Action<NetworkObject> OnPlayerSpawned;
        
        private PlayerSettings data;
        private PlayerInteracter interacter;
        private NetworkInputData inputs;
        [SerializeField] private CameraStrategy mainCamera;

        private void Awake()
        {
            data = SettingsSystem.Instance.PlayerSetting;

            interacter = GetComponent<PlayerInteracter>();

            MovementAwake();
        }

        private void Start()
        {
            NetworkSystem.Instance.OnPlayerLeftEvent += PlayerLeft;
        }

        public override void Spawned()
        {
            base.Spawned();
            if (mainCamera == null && UnityEngine.Camera.main != null) mainCamera = UnityEngine.Camera.main.GetComponentInParent<CameraStrategy>();
            mainCamera.AddTarget(gameObject);
            OnPlayerSpawned?.Invoke(Object);
        }

        public override void FixedUpdateNetwork()
        {
            if (GetInput(out NetworkInputData inputData))
            {
                inputs = inputData;
            }
            
            MoveUpdate(inputs);
            if (inputs.IsInteract)
            {
                interacter.InteractWithClosestInteraction();
            }
        }

        private void PlayerLeft(NetworkRunner networkRunner, PlayerRef playerRef)
        {
            if (playerRef == Object.InputAuthority)
                networkRunner.Despawn(Object);
        }

        private void OnCollisionEnter(Collision collision) // TODO Replace with the dive feature (temporary)
        {
            if (collision.gameObject.CompareTag(TAG) || collision.gameObject.CompareTag(AIEntity.TAG))
            {
                var networkObject = collision.gameObject.GetComponentInEntity<NetworkObject>();
                Debug.Assert(networkObject, $"A player or an AI should have a {nameof(NetworkObject)}");
                RPC_DropItems(networkObject.Id);
            }
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_DropItems(NetworkId entityNetworkId)
        {
            var networkObject = NetworkSystem.Instance.FindObject(entityNetworkId);
            var inventory = networkObject.GetComponentInEntity<Inventory>();
            Debug.Assert(inventory, $"A player or an AI should have an {nameof(Inventory)}");
            inventory.DropEverything();
        }
        
        private bool ValidateIfHasTag()
        {
            return gameObject.CompareTag(TAG);
        }
    }
}