using System;
using System.Threading.Tasks;
using Fusion;
using Interfaces;
using Sirenix.OdinInspector;
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
        public event Action OnArchetypeChanged;

        [SerializeField] private CameraStrategy mainCamera;

        private PlayerSettings data;
        private PlayerInteracter interacter;
        private Inventory inventory;

        private TickTimer immunityTimer;
        private NetworkBool isImmune;
        private bool inMenu;

        public int PlayerID { get; private set; }

        [Networked(OnChangedTargets = OnChangedTargets.All)] public NetworkBool IsReady { get; set; }
        [Networked] [Capacity(128)] public string TeamId { get; set; }
        [Networked] public int PlayerScore { get; set; }
        
        [Networked(OnChanged = nameof(OnNetworkArchetypeChanged))]
        public Archetype Archetype { get; private set; }

        private void OnAwake()
        {
            data = SettingsSystem.Instance.GetPlayerSettings(Archetype.Base);
            print(data.PlayerArchetype);
            interacter = GetComponent<PlayerInteracter>();
            inventory = GetComponent<Inventory>();

            inventory.AssignVelocityObject(this);

            AssignRandomArchetype(); // TODO Assign from hub
            
            MovementAwake();
            RagdollAwake();
        }

        private void AssignRandomArchetype()
        {
            if (Object.HasStateAuthority)
                Archetype = ((Archetype[])Enum.GetValues(typeof(Archetype))).RandomElement();
        }

        private void ImmunityTimerOnTimerEnd() => isImmune = false;

        public override async void Spawned()
        {
            base.Spawned();
            OnAwake();
            InitThrow();

            PlayerID = Object.InputAuthority.PlayerId;
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

            if (NetworkSystem.Instance.IsHost)
            {
                TeamSystem.Instance.AssignTeam(this);
            }
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

                if (Runner.IsForward)
                {
                    if (inputData.IsInteractOnce && !inMenu)
                    {
                        interacter.InteractWithClosestInteraction();
                    }

                    if (inputData.IsReadyOnce && !inMenu)
                    {
                        IsReady = !IsReady;
                        Debug.Log($"Toggle ready for player id {PlayerID} : {IsReady}");
                    }

                    if (inputData.IsMenu)
                    {
                        inMenu = !inMenu;
                        if (inMenu) IsReady = false;
                        OnMenuPressed?.Invoke();
                    }

                    if (immunityTimer.Expired(Runner)) ImmunityTimerOnTimerEnd();
                }

                AnimationUpdate();
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

        public void ExternalHit(float overrideHitDuration = -1f)
        {
            RPC_GetHitAndDropItems(Object.Id, true, transform.forward, data.DashForceApplied, overrideHitDuration);
        }

        [Rpc] 
        private void RPC_GetHitAndDropItems(NetworkId entityNetworkId, NetworkBool isPlayer, Vector3 forceDirection = default, float forceMagnitude = default, float overrideHitDuration = -1f)
        {
            var networkObject = NetworkSystem.Instance.FindObject(entityNetworkId);
            Inventory inv;
            if (isPlayer)
            {
                var player = networkObject.GetComponent<PlayerEntity>();
                if (player.isImmune) return;
                inv = player.inventory;
                player.ResetVelocity();

                player.Hit(forceDirection, forceMagnitude, overrideHitDuration);

                immunityTimer = TickTimer.CreateFromSeconds(Runner,data.ImmunityTime);
                isImmune = true;
            }
            else
            {
                var aiEntity = networkObject.GetComponentInEntity<AIEntity>();
                Debug.Assert(aiEntity);
                inv = aiEntity.Inventory;
                aiEntity.Hit(gameObject, overrideHitDuration);
            }

            Debug.Assert(inv, $"A player or an AI should have an {nameof(Inventory)}");
            inv.DropEverything(Velocity.normalized + Vector3.up * 0.5f, 1f);
        }

        private void UpdateArchetype()
        {
            data = SettingsSystem.Instance.GetPlayerSettings(Archetype);
            OnArchetypeChanged?.Invoke();
        }

        static private void OnNetworkArchetypeChanged(Changed<PlayerEntity> changed)
        {
            changed.Behaviour.UpdateArchetype();
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
        
        private bool showDebugMenu;
        
        [Button("ToggleDebugMenu")]
        private void ToggleDebugMenu()
        {
            showDebugMenu = !showDebugMenu;
        }

        private void OnGUI()
        {
            if (Runner.IsRunning && Object.HasInputAuthority && showDebugMenu)
            {
                if (GUI.Button(new Rect(0, 0, 200, 40), "Base"))
                {
                    RPC_DebugChangeArchetypeOnHost(Archetype.Base);
                }

                if (GUI.Button(new Rect(0, 40, 200, 40), "Runner"))
                {
                    RPC_DebugChangeArchetypeOnHost(Archetype.Runner);
                }

                if (GUI.Button(new Rect(0, 80, 200, 40), "Thrower"))
                {
                    RPC_DebugChangeArchetypeOnHost(Archetype.Thrower);
                }

                if (GUI.Button(new Rect(0, 120, 200, 40), "Dasher"))
                {
                    RPC_DebugChangeArchetypeOnHost(Archetype.Dasher);
                }
            }
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_DebugChangeArchetypeOnHost(Archetype archetype)
        {
            Archetype = archetype;
        }
#endif
    }
}