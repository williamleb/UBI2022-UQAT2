using System;
using System.Threading.Tasks;
using Fusion;
using Scriptables;
using Systems;
using Systems.Network;
using Units.Camera;
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
        public event Action OnMenuPressed;
        public int PlayerID { get; private set; }
        
        [SerializeField] private CameraStrategy mainCamera;
        [SerializeField] private NetworkObject scorePrefab;
        
        private PlayerSettings data;
        private PlayerInteracter interacter;
        private Inventory inventory;
        private NetworkInputData inputs;
        
        [Networked] private NetworkId ScoreObjectId { get; set; }

        private void Awake()
        {
            data = SettingsSystem.Instance.PlayerSetting;

            interacter = GetComponent<PlayerInteracter>();
            inventory = GetComponent<Inventory>();

            MovementAwake();
        }

        private void Start()
        {
        }

        public override async void Spawned()
        {
            base.Spawned();

            gameObject.name = $"Player{Object.InputAuthority.PlayerId}";
            
            if (mainCamera == null && UnityEngine.Camera.main != null) mainCamera = UnityEngine.Camera.main.GetComponentInParent<CameraStrategy>();
            if (!Object.HasInputAuthority)
            {
                mainCamera!.gameObject.Hide();
            }
            else
            {
                mainCamera!.AddTarget(gameObject);
            }

            await Task.Delay(100);
            OnPlayerSpawned?.Invoke(Object);
            
            PlayerSystem.Instance.AddPlayer(this);

            if (Object.HasStateAuthority)
                SpawnScore();
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (Object.HasStateAuthority)
                DespawnScore();
        }

        private void SpawnScore()
        {
            if (!scorePrefab)
            {
                Debug.LogWarning($"Could not spawn a score for player {Object.InputAuthority.PlayerId} because it didn't have a valid {nameof(scorePrefab)}");
                return;
            }
            
            var scoreObject = Runner.Spawn(scorePrefab, Vector3.zero, Quaternion.identity, Object.InputAuthority);
            ScoreObjectId = scoreObject.Id;
        }

        private void DespawnScore()
        {
            var scoreObject = Runner.FindObject(ScoreObjectId);
            Runner.Despawn(scoreObject);
            ScoreObjectId = new NetworkId();
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

            if (inputs.IsMenu)
            {
                OnMenuPressed?.Invoke();
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
                inv = networkObject.GetComponent<Inventory>();
            }
            Debug.Assert(inv, $"A player or an AI should have an {nameof(Inventory)}");
            inv.DropEverything();
            
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