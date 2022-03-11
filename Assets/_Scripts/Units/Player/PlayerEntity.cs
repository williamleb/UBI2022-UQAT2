using System;
using System.Threading.Tasks;
using Fusion;
using Interfaces;
using Systems;
using Systems.Network;
using Systems.Settings;
using Units.AI;
using Units.Camera;
using UnityEditor;
using UnityEngine;
using Utilities.Extensions;
using Utilities.Unity;
using PlayerSettings = Systems.Settings.PlayerSettings;
using TickTimer = Utilities.TickTimer;

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

        [SerializeField] private CameraStrategy mainCamera;

        private PlayerSettings data;
        private PlayerInteracter interacter;
        private Inventory inventory;

        private TickTimer immunityTimer;
        private NetworkBool isImmune;

        public int PlayerID { get; private set; }
        public Vector3 Velocity => nRb.Rigidbody.velocity;

        [Networked(OnChangedTargets = OnChangedTargets.All)] public NetworkBool IsReady { get; set; }

        private void OnAwake()
        {
            data = SettingsSystem.Instance.PlayerSetting.RandomElement();
            print(data.PlayerArchetypes);
            interacter = GetComponent<PlayerInteracter>();
            inventory = GetComponent<Inventory>();

            inventory.AssignVelocityObject(this);

            immunityTimer = new TickTimer(data.ImmunityTime);
            immunityTimer.OnTimerEnd += ImmunityTimerOnTimerEnd;

            MovementAwake();
            DashAwake();
            RagdollAwake();
        }

        private void ImmunityTimerOnTimerEnd() => isImmune = false;

        public override async void Spawned()
        {
            base.Spawned();
            OnAwake();
            InitThrow();

            gameObject.name = $"Player{Object.InputAuthority.PlayerId}";

            if (Object.HasInputAuthority)
            {
                mainCamera.Init(data.PlayerCameraSetting);
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
                MoveUpdate(inputData);
                DashUpdate(inputData);
                ThrowUpdate(inputData);

                if (inputData.IsInteractOnce && Runner.IsForward)
                {
                    interacter.InteractWithClosestInteraction();
                }

                if (inputData.IsReadyOnce)
                {
                    IsReady = !IsReady;
                    Debug.Log($"Toggle ready for player id {PlayerID} : {IsReady}");
                }

                if (inputData.IsMenu)
                {
                    OnMenuPressed?.Invoke();
                }

                AnimationUpdate();
                immunityTimer.Tick(Runner.DeltaTime);
            }
        }

        public async void TriggerDespawn()
        {
            //await Task.Delay(300); // wait for effects

            if (Object == null)
            {
                return;
            }

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
            RPC_GetHitAndDropItems(Object.Id, true, transform.forward, data.DashForceApplied);
        }

        [Rpc]
        private void RPC_GetHitAndDropItems(NetworkId entityNetworkId, NetworkBool isPlayer, Vector3 forceDirection = default, float forceMagnitude = default)
        {
            var networkObject = NetworkSystem.Instance.FindObject(entityNetworkId);
            Inventory inv;
            if (isPlayer)
            {
                var player = networkObject.GetComponent<PlayerEntity>();
                if (player.isImmune) return;
                inv = player.inventory;
                player.ResetVelocity();

                player.Hit(forceDirection, forceMagnitude);

                immunityTimer.Reset();
                isImmune = true;
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
                Debug.LogWarning(
                    $"Player {thisGameObject.name} should have the tag {Tags.PLAYER}. Instead, it has {thisGameObject.tag}");

            if (!thisGameObject.AssignLayerIfDoesNotHaveIt(Layers.GAMEPLAY))
                Debug.LogWarning(
                    $"Player {thisGameObject.name} should have the layer {Layers.GAMEPLAY} ({Layers.NAME_GAMEPLAY}). Instead, it has {thisGameObject.layer}");
        }
#endif
    }
}