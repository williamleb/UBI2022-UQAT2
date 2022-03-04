using System;
using System.Threading.Tasks;
using Fusion;
using Interfaces;
using Sirenix.OdinInspector;
using Systems;
using Systems.Network;
using Units.AI;
using Units.Camera;
using UnityEditor;
using UnityEngine;
using Utilities.Extensions;
using Utilities.Unity;
using PlayerSettings = Scriptables.PlayerSettings;

namespace Units.Player
{
    [RequireComponent(typeof(PlayerInteracter))]
    [RequireComponent(typeof(PlayerInputHandler))]
    [RequireComponent(typeof(Inventory))]
    public partial class PlayerEntity : NetworkBehaviour, IVelocityObject
    {
        public static event Action<NetworkObject> OnPlayerSpawned;
        public static event Action<NetworkObject> OnPlayerDespawned;
        public event Action OnMenuPressed;
        
        [SerializeField][Required] private CameraStrategy mainCamera;
        
        private PlayerSettings data;
        private PlayerInteracter interacter;
        private Inventory inventory;
        private NetworkInputData inputs;

        private bool isThrowing = false;
        
        public int PlayerID { get; private set; }
        public Vector3 Velocity => nRb.Rigidbody.velocity;
        
        private void Awake()
        {
            data = SettingsSystem.Instance.PlayerSetting;

            interacter = GetComponent<PlayerInteracter>();
            inventory = GetComponent<Inventory>();
            
            inventory.AssignVelocityObject(this);

            MovementAwake();
        }

        public override async void Spawned()
        {
            base.Spawned();

            gameObject.name = $"Player{Object.InputAuthority.PlayerId}";

            if (Object.HasInputAuthority)
            {
                mainCamera.AddTarget(gameObject);
            }
            else
            {
                mainCamera.gameObject.Hide();
            }

            await Task.Delay(100);
            OnPlayerSpawned?.Invoke(Object);
            
            PlayerSystem.Instance.AddPlayer(this);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            OnPlayerDespawned?.Invoke(Object);
        }

        public override void FixedUpdateNetwork()
        {
            if (GetInput(out NetworkInputData inputData))
            {
                SetMoveInput(inputData);
                if (inputData.IsInteractOnce && Runner.IsForward)
                {
                    interacter.InteractWithClosestInteraction();
                }

                UpdateThrow(inputData);

                if (inputData.IsMenu)
                {
                    OnMenuPressed?.Invoke();
                }
            }
            MoveUpdate();
        }

        private void UpdateThrow(NetworkInputData inputData)
        {
            // TODO This is temp until we have the right logic for the throw
            if (inputData.IsThrow)
            {
                isThrowing = true;
            }
            else
            {
                if (isThrowing)
                {
                    isThrowing = false;
                    inventory.DropEverything(transform.forward + Vector3.up * 0.25f, 2f);
                }
            }
        }

        public async void TriggerDespawn()
        {
            //await Task.Delay(300); // wait for effects

            if (Object == null) { return; }

            if (Object.HasStateAuthority)
            {
                Runner.Despawn(Object);
            }
            else if (Runner.IsSharedModeMasterClient)
            {
                Object.RequestStateAuthority();

                while (Object.HasStateAuthority == false)
                {
                    await Task.Delay(100); // wait for Auth transfer
                }

                if (Object.HasStateAuthority)
                {
                    Runner.Despawn(Object);
                }
            }
        }

        public void ExternalHit()
        {
            RPC_DropItems(Object.Id, true);
        }

        [Rpc]
        private void RPC_DropItems(NetworkId entityNetworkId, NetworkBool isPlayer)
        {
            var networkObject = NetworkSystem.Instance.FindObject(entityNetworkId);
            Inventory inv;
            if (isPlayer)
            {
                var player = networkObject.GetComponent<PlayerEntity>();
                inv = player.inventory;
                player.Hit();
            }
            else
            {
                var aiEntity = networkObject.GetComponentInEntity<AIEntity>();
                Debug.Assert(aiEntity);
                inv = aiEntity.Inventory;
                aiEntity.Hit();
            }

            Debug.Assert(inv, $"A player or an AI should have an {nameof(Inventory)}");
            inv.DropEverything(Velocity.normalized + Vector3.up * 0.5f, 1f);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!Application.isPlaying)
                EditorApplication.delayCall += AssignTagAndLayer;
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