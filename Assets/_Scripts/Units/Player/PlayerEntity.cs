using System;
using System.Threading.Tasks;
using Canvases.Menu;
using Fusion;
using Ingredients.Volumes.WorldObjects;
using Managers.Game;
using Sirenix.OdinInspector;
using Systems;
using Systems.Level;
using Systems.Network;
using Systems.Settings;
using Systems.Teams;
using Units.AI;
using Units.Customization;
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
    public partial class PlayerEntity : NetworkBehaviour, IWorldObject
    {
        public static event Action<NetworkObject> OnPlayerSpawned;
        public static event Action<NetworkObject> OnPlayerDespawned;
        public event Action OnArchetypeChanged;
        public event Action OnTeamChanged;
        
        public event Action OnInventoryChanged
        {
            add => inventory.OnInventoryChanged += value;
            remove => inventory.OnInventoryChanged -= value;
        }

        private PlayerSettings data;
        private PlayerInteracter interacter;
        private Inventory inventory;
        private PlayerCustomization customization;
        private PlayerInputHandler playerInputHandler;

        private TickTimer immunityTimer;
        private NetworkBool isImmune;
        private string currentDeviceName;

        public int PlayerId { get; private set; }
        [Networked] private NetworkInputData Inputs { get; set; }
        public PlayerCustomization Customization => customization;

        [Networked(OnChanged = nameof(OnNetworkTeamIdChanged))]
        [Capacity(128)]
        public string TeamId { get; set; }

        [Networked] public int PlayerScore { get; set; }
        [Networked] private bool InCustomization { get; set; }
        [Networked] private bool InMenu { get; set; }

        [Networked(OnChanged = nameof(OnNetworkArchetypeChanged))]
        public Archetype Archetype { get; private set; }
        public Vector3 Position => transform.position;

        public bool IsHoldingHomework => inventory.HasHomework;

        private bool IsGameFinished =>
            GameManager.HasInstance && GameManager.Instance.CurrentState == GameState.Finished;

        private bool CanInteract => !InMenu && !InCustomization && !IsDashing && !isRagdoll && CanMove;

        private bool IsUsingGamePad => currentDeviceName == "Gamepad";

        private void Awake()
        {
            //TODO Hide player model while loading everything
        }

        private void OnAwake()
        {
            data = SettingsSystem.Instance.GetPlayerSettings(Archetype.Base);
            interacter = GetComponent<PlayerInteracter>();
            inventory = GetComponent<Inventory>();
            customization = GetComponent<PlayerCustomization>();
            playerInputHandler = GetComponent<PlayerInputHandler>();

            inventory.AssignVelocityObject(this);

            AssignArchetype(Archetype.Base);

            MovementAwake();
            RagdollAwake();
            SoundAwake();
        }

        private void OnEnable()
        {
            LevelSystem.OnGameLoad += DeactivateMenuAndCustomization;

            if (GameManager.HasInstance)
                GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
        }

        private void OnDisable()
        {
            LevelSystem.OnGameLoad -= DeactivateMenuAndCustomization;

            if (GameManager.HasInstance)
                GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
        }

        private void OnGameStateChanged(GameState state)
        {
            if (state == GameState.Finished)
            {
                DeactivateMenuAndCustomization();
                SetImmunity(true);
            }
        }

        public void AssignArchetype(Archetype archetype)
        {
            if (Object.HasStateAuthority)
                Archetype = archetype;
        }

        private void ImmunityTimerOnTimerEnd()
        {
            isImmune = false;
            immunityTimer = TickTimer.None;
        }

        public override async void Spawned()
        {
            base.Spawned();
            OnAwake();
            InitDash();
            InitCollision();
            InitThrow();
            InitCamera();
            InitSound();
            InitReady();
            InitAnim();


            PlayerId = Object.InputAuthority.PlayerId;
            gameObject.name = $"Player{Object.InputAuthority.PlayerId}";

            await Task.Delay(100);
            OnPlayerSpawned?.Invoke(Object);
            
            PlayerSystem.Instance.AddPlayer(this);

            if (NetworkSystem.Instance.IsHost)
            {
                TeamSystem.Instance.AssignTeam(this);
            }
            if (Object.HasInputAuthority)
                playerInputHandler.OnInputDeviceChanged += OnInputDeviceChanged;
            NetworkSystem.OnSceneLoadStartEvent += OnSceneLoadStartEvent;
            NetworkSystem.OnSceneLoadDoneEvent += OnSceneLoadDoneEvent;
            
            //TODO show player after everything is loaded
            
        }

        private async void OnSceneLoadDoneEvent(NetworkRunner runner)
        {
            await Task.Delay(500);
            SetImmunity(false);
            if (GameManager.HasInstance)
                GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
        }

        private void OnSceneLoadStartEvent(NetworkRunner runner)
        {
            SetImmunity(true);
            inventory.DropEverything();
            StopCustomization();
            if (GameManager.HasInstance)
                GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            OnPlayerDespawned?.Invoke(Object);
            readyMarker.Deactivate();
            TerminateSound();
        }

        public override void FixedUpdateNetwork()
        {
            if (IsGameFinished) return;

            ThrowUpdateOnAllClients();

            if (GetInput(out NetworkInputData inputData))
            {
                Inputs = inputData;
            }

            MoveUpdate();
            DashUpdate();
            ThrowUpdate();
            ReadyUpdate();

            if (Runner.IsForward)
            {
                if (Inputs.IsInteractOnce && CanInteract)
                {
                    interacter.InteractWithClosestInteraction();
                    CancelAimingAndThrowing();
                }

                if (!InMenu && !InCustomization)
                {
                    IsDancing = Inputs.IsDanceOnce;
                }

                if (Inputs.IsMenu && Object.HasInputAuthority)
                {
                    if (!InCustomization)
                    {
                        if (InMenu)
                            CloseMenu();
                        else
                            OpenMenu();
                    }
                    else
                    {
                        StopCustomization();
                    }
                }

                if (immunityTimer.Expired(Runner)) ImmunityTimerOnTimerEnd();
            }

            AnimationUpdate();
            RagdollUpdate();
        }

    public async void TriggerDespawn()
    {
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

        NetworkSystem.OnSceneLoadStartEvent -= OnSceneLoadStartEvent;
        NetworkSystem.OnSceneLoadDoneEvent -= OnSceneLoadDoneEvent;
    }

    public void ExternalHit(float overrideHitDuration = -1f, Vector3 direction = default)
    {
        RPC_GetHitAndDropItems(Object.Id, true, direction, data.DashForceApplied, overrideHitDuration);
    }

    [Rpc]
    private void RPC_GetHitAndDropItems(NetworkId entityNetworkId, NetworkBool isPlayer,
        Vector3 forceDirection = default, float forceMagnitude = default, float overrideHitDuration = -1f, NetworkBool fumble = default)
    {
        var networkObject = NetworkSystem.Instance.FindObject(entityNetworkId);
        Inventory inv;
        if (isPlayer)
        {
            var player = networkObject.GetComponentInChildren<PlayerEntity>();
            if (player.isImmune) return;
            inv = player.inventory;
            player.ResetVelocity();

            player.Hit(forceDirection, forceMagnitude, overrideHitDuration, fumble);

            player.immunityTimer = TickTimer.CreateFromSeconds(Runner, data.ImmunityTime);
            player.isImmune = true;
        }
        else
        {
            var aiEntity = networkObject.GetComponentInEntity<AIEntity>();
            Debug.Assert(aiEntity);
            if (aiEntity.IsImmune) return;
            inv = aiEntity.Inventory;
            aiEntity.Hit(gameObject, overrideHitDuration);
            aiEntity.IsImmune = true;
        }

        Debug.Assert(inv, $"A player or an AI should have an {nameof(Inventory)}");
        inv.DropEverything(Velocity.normalized + Vector3.up * 0.5f, 1f);

        if (Object.HasStateAuthority && entityNetworkId != Object.Id)
            RPC_DetectSuccessfulHitOnAllClients();
    }

    private void UpdateArchetype()
    {
        data = SettingsSystem.Instance.GetPlayerSettings(Archetype);
        OnArchetypeChanged?.Invoke();
    }

    private void UpdateTeam() => OnTeamChanged?.Invoke();

    private void DeactivateMenuAndCustomization()
    {
        if (!Object)
            return;

        if (!Object.HasInputAuthority)
            return;

        CloseMenu();
        StopCustomization();
    }

    private void OpenMenu()
    {
        if (InMenu)
            return;

        if (!MenuManager.HasInstance)
            return;

        if (!MenuManager.Instance.ShowMenuForPlayer(MenuManager.Menu.Game, this))
            return;

        RPC_ChangeInMenu(true);
        ResetReady();
    }

    public void CloseMenu()
    {
        if (!InMenu)
            return;

        if (!MenuManager.HasInstance)
            return;

        var gameInTransition = MenuManager.Instance.IsInTransition(MenuManager.Menu.Game);
        var optionsInTransition = MenuManager.Instance.IsInTransition(MenuManager.Menu.Options);
        var controlsInTransition = MenuManager.Instance.IsInTransition(MenuManager.Menu.Controls);
        if (gameInTransition || optionsInTransition || controlsInTransition)
            return;

        MenuManager.Instance.HideMenu(MenuManager.Menu.Game);
        MenuManager.Instance.HideMenu(MenuManager.Menu.Options);
        MenuManager.Instance.HideMenu(MenuManager.Menu.Controls);
        MenuManager.Instance.UnfocusEverything();
        RPC_ChangeInMenu(false);
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

        ResetReady();
        RPC_ChangeInCustomization(true);
        customizationCamera.Activate();
    }

    public void StopCustomization()
    {
        if (!InCustomization)
            return;

        if (!MenuManager.HasInstance)
            return;

        if (MenuManager.Instance.HasMenu(MenuManager.Menu.Customization) && !MenuManager.Instance.HideMenu(MenuManager.Menu.Customization))
            return;

        RPC_ChangeInCustomization(false);
        mainCamera.Activate();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_ChangeInCustomization(NetworkBool inCustomization)
    {
        InCustomization = inCustomization;
        SetImmunity(inCustomization);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_ChangeInMenu(NetworkBool inMenu)
    {
        InMenu = inMenu;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_DetectSuccessfulHitOnAllClients()
    {
        if (Object.HasInputAuthority)
            PlayDashCollisionSoundLocally();
        else
            StopDashSoundLocally();
    }

    private void ResetPlayerState() => ResetThrowState();

    private static void OnNetworkArchetypeChanged(Changed<PlayerEntity> changed) => changed.Behaviour.UpdateArchetype();

    private static void OnNetworkTeamIdChanged(Changed<PlayerEntity> changed) => changed.Behaviour.UpdateTeam();
    private void OnInputDeviceChanged(string newDeviceName) => currentDeviceName = newDeviceName;

    private void OnDestroy()
    {
        AnimOnDestroy();
        ReadyOnDestroy();
        if (Object && Object.HasInputAuthority)
            playerInputHandler.OnInputDeviceChanged -= OnInputDeviceChanged;
        NetworkSystem.OnSceneLoadStartEvent -= OnSceneLoadStartEvent;
        NetworkSystem.OnSceneLoadDoneEvent -= OnSceneLoadDoneEvent;
    }
    
    public void OnEscapedWorld()
    {
        if (!Object || !Object.HasStateAuthority)
            return;
        
        if (!PlayerSystem.HasInstance)
            return;
        
        PlayerSystem.Instance.SetPlayerPositionToSpawn(this);
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

        if (!thisGameObject.AssignLayerIfDoesNotHaveIt(Layers.PLAYER))
            Debug.LogWarning(
                $"Player {thisGameObject.name} should have the layer {Layers.PLAYER} ({Layers.NAME_PLAYER}). Instead, it has {thisGameObject.layer}");
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