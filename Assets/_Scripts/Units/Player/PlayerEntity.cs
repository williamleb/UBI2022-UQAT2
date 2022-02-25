using System;
using System.Threading.Tasks;
using Fusion;
using Scriptables;
using Systems;
using Systems.Network;
using Units.Camera;
using UnityEngine;
using Utilities.Tags;

namespace Units.Player
{
    [RequireComponent(typeof(PlayerInteracter))]
    [RequireComponent(typeof(PlayerInputHandler))]
    [RequireComponent(typeof(Inventory))]
    public partial class PlayerEntity : NetworkBehaviour
    {
        public static event Action<NetworkObject> OnPlayerSpawned;
        public event Action OnMenuPressed;
        
        private PlayerSettings data;
        private PlayerInteracter interacter;
        private Inventory inventory;
        private NetworkInputData inputs;
        [SerializeField] private CameraStrategy mainCamera;

        private void Awake()
        {
            data = SettingsSystem.Instance.PlayerSetting;

            interacter = GetComponent<PlayerInteracter>();
            inventory = GetComponent<Inventory>();

            MovementAwake();
        }

        private void Start()
        {
            NetworkSystem.Instance.OnPlayerLeftEvent += PlayerLeft;
        }

        public override async void Spawned()
        {
            base.Spawned();
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

        private void PlayerLeft(NetworkRunner networkRunner, PlayerRef playerRef)
        {
            if (playerRef == Object.InputAuthority)
                networkRunner.Despawn(Object);
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
        
        private bool OnValidate()
        {
            AssignPlayerTagIfDoesNotHaveIt();
        }

        private void AssignPlayerTagIfDoesNotHaveIt()
        {
            var thisGameObject = gameObject;
            if (thisGameObject.CompareTag(Tags.UNTAGGED))
            {
                gameObject.tag = Tags.PLAYER;
            }

            if (!thisGameObject.CompareTag(Tags.PLAYER))
            {
                Debug.LogWarning($"Player {thisGameObject.name} should have the tag {Tags.PLAYER}. Instead, it has {thisGameObject.tag}");
            }
        }
    }
}