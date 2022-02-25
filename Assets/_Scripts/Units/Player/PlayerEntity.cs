using System;
using Fusion;
using Scriptables;
using Systems;
using Systems.Network;
using Units.Camera;
using Units.AI;
using UnityEngine;
using Utilities.Extensions;
using Utilities.Unity;

namespace Units.Player
{
    [RequireComponent(typeof(PlayerInteracter))]
    [RequireComponent(typeof(PlayerInputHandler))]
    [RequireComponent(typeof(Inventory))]
    public partial class PlayerEntity : NetworkBehaviour
    {
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
                interacter.InteractWithClosestInteraction(inputs.IsInteractOnce);
            }
        }

        private void PlayerLeft(NetworkRunner networkRunner, PlayerRef playerRef)
        {
            if (playerRef == Object.InputAuthority)
                networkRunner.Despawn(Object);
        }

        private void OnCollisionEnter(Collision collision) // TODO Replace with the dive feature
        {
            if (!Object.HasInputAuthority)
                return;
            
            if (collision.gameObject.CompareTag(Tags.PLAYER) || collision.gameObject.CompareTag(AIEntity.TAG))
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

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!Application.isPlaying)
                UnityEditor.EditorApplication.delayCall += AssignTagAndLayer;
        }

        private void AssignTagAndLayer()
        {
            if (this == null)
                return;

            var thisGameObject = gameObject;
            
            if (!thisGameObject.AssignTagIfDoesNotHaveIt(Tags.PLAYER))
                Debug.LogWarning($"Player {thisGameObject.name} should have the tag {Tags.PLAYER}. Instead, it has {thisGameObject.tag}");
            
            if (!thisGameObject.AssignLayerIfDoesNotHaveIt(Layers.GAMEPLAY))
                Debug.LogWarning($"Player {thisGameObject.name} should have the layer {Layers.GAMEPLAY} ({Layers.NAME_GAMEPLAY}). Instead, it has {thisGameObject.layer}");
        }
#endif
    }
}