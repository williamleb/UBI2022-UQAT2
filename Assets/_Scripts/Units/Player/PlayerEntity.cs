using System;
using System.Threading.Tasks;
using Canvases.Menu;
using Fusion;
using Interfaces;
using Sirenix.OdinInspector;
using Systems;
using Systems.Network;
using Systems.Settings;
using Systems.Teams;
using Units.AI;
using Units.Player.Customisation;
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
    [RequireComponent(typeof(PlayerCustomization))]
    public partial class PlayerEntity : NetworkBehaviour, IVelocityObject
    {
        public static event Action<NetworkObject> OnPlayerSpawned;
        public static event Action<NetworkObject> OnPlayerDespawned;
        public event Action OnMenuPressed;
        public event Action OnArchetypeChanged;
        public event Action OnTeamChanged;
        
        private PlayerSettings data;
        private PlayerInteracter interacter;
        private Inventory inventory;
        private PlayerCustomization customization;

        private TickTimer immunityTimer;
        private NetworkBool isImmune;

        public int PlayerId { get; private set; }
        public PlayerCustomization Customization => customization;

        [Networked(OnChanged = nameof(OnNetworkTeamIdChanged))] [Capacity(128)] public string TeamId { get; set; }
        [Networked] public int PlayerScore { get; set; }
        [Networked] private bool InCustomization { get; set; }
        [Networked] private bool InMenu { get; set; }

        [Networked(OnChanged = nameof(OnNetworkArchetypeChanged))]
        public Archetype Archetype { get; private set; }

        private void OnAwake()
        {
            data = SettingsSystem.Instance.GetPlayerSettings(Archetype.Base);
            print(data.PlayerArchetype);
            interacter = GetComponent<PlayerInteracter>();
            inventory = GetComponent<Inventory>();
            customization = GetComponent<PlayerCustomization>();

            inventory.AssignVelocityObject(this);

            AssignBaseArchetype();
            
            MovementAwake();
            RagdollAwake();
            SoundAwake();
        }

        private void OnEnable()
        {
            LevelSystem.Instance.OnGameLoad += DeactivateMenuAndCustomization;
        }
        
        private void OnDisable()
        {
            LevelSystem.Instance.OnGameLoad -= DeactivateMenuAndCustomization;
        }

        private void AssignBaseArchetype()
        {
            if (Object.HasStateAuthority)
                Archetype = Archetype.Base;
        }

        public void AssignArchetype(Archetype archetype)
        {
            if (Object.HasStateAuthority)
                Archetype = archetype;
        }

        private void ImmunityTimerOnTimerEnd() => isImmune = false;

        public override async void Spawned()
        {
            base.Spawned();
            OnAwake();
            InitThrow();
            InitCamera();
            InitSound();
            InitReady();

            PlayerId = Object.InputAuthority.PlayerId;
            gameObject.name = $"Player{Object.InputAuthority.PlayerId}";

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
            TerminateSound();
        }

        public override void FixedUpdateNetwork()
        {
            ThrowUpdateOnAllClients();
            
            if (GetInput(out NetworkInputData inputData))
            {
                MoveUpdate(inputData);
                DashUpdate(inputData);
                ThrowUpdate(inputData);
                ReadyUpdate(inputData);

                if (Runner.IsForward)
                {
                    if (inputData.IsInteractOnce && !InMenu && !InCustomization && !IsDashing)
                    {
                        interacter.InteractWithClosestInteraction();
                    }

                    if (inputData.IsDanceOnce && !InMenu && !InCustomization)
                    {
                        PlayDancingAnim();
                    }

                    if (inputData.IsMenu && !InCustomization && Object.HasInputAuthority)
                    {
                        if (InMenu)
                            CloseMenu();
                        else 
                            OpenMenu();
                    }

                    if (immunityTimer.Expired(Runner)) ImmunityTimerOnTimerEnd();
                }

                AnimationUpdate();
                RagdollUpdate();
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
                var player = networkObject.GetComponentInChildren<PlayerEntity>();
                if (player.isImmune) return;
                inv = player.inventory;
                player.ResetVelocity();

                player.Hit(forceDirection, forceMagnitude, overrideHitDuration);

                player.immunityTimer = TickTimer.CreateFromSeconds(Runner,data.ImmunityTime);
                player.isImmune = true;
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
        
        private void UpdateTeam()
        {
            OnTeamChanged?.Invoke();
        }
        
        private void DeactivateMenuAndCustomization()
        {
            if (!Object)
                return;

            if (!Object.HasInputAuthority)
                return;
            
            CloseMenu();
            StopCustomization();
        }

        public void OpenMenu()
        {
            if (InMenu)
                return;
            
            if (!MenuManager.HasInstance)
                return;

            if (!MenuManager.Instance.ShowMenuForPlayer(MenuManager.Menu.Game, this))
                return;
            
            RPC_ChangeInMenu(true);
            OnMenuPressed?.Invoke();
            ResetReady();
        }

        public void CloseMenu()
        {
            if (!InMenu)
                return;
            
            if (!MenuManager.HasInstance)
                return;

            MenuManager.Instance.HideMenu(MenuManager.Menu.Game);
            MenuManager.Instance.HideMenu(MenuManager.Menu.Options);
            MenuManager.Instance.HideMenu(MenuManager.Menu.Controls);
            RPC_ChangeInMenu(false);
            OnMenuPressed?.Invoke();
        }

        public void StartCustomization()
        {
            if (InCustomization)
                return;

            if (!MenuManager.HasInstance)
                return;

            if (!MenuManager.Instance.ShowMenuForPlayer(MenuManager.Menu.Customization, this))
                return;
            
            CloseMenu();

            //ResetReady();
            RPC_ChangeInCustomization(true);
            customizationCamera.Activate();
        }

        public void StopCustomization()
        {
            if (!InCustomization)
                return;
            
            if (!MenuManager.HasInstance)
                return;
            
            MenuManager.Instance.HideMenu(MenuManager.Menu.Customization);

            RPC_ChangeInCustomization(false);
            mainCamera.Activate();
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_ChangeInCustomization(NetworkBool inCustomization)
        {
            InCustomization = inCustomization;
        }
        
        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_ChangeInMenu(NetworkBool inMenu)
        {
            InMenu = inMenu;
        }

        private static void OnNetworkArchetypeChanged(Changed<PlayerEntity> changed)
        {
            changed.Behaviour.UpdateArchetype();
        }
        
        private static void OnNetworkTeamIdChanged(Changed<PlayerEntity> changed)
        {
            changed.Behaviour.UpdateTeam();
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
                
                if (GUI.Button(new Rect(0, 160, 200, 40), "New Team"))
                {
                    TeamSystem.Instance.AssignTeam(this);
                }
                
                if (GUI.Button(new Rect(0, 200, 200, 40), "Custom on"))
                {
                    customizationCamera.Activate(); 
                }
                
                if (GUI.Button(new Rect(0, 240, 200, 40), "Custom off"))
                {
                    mainCamera.Activate();
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