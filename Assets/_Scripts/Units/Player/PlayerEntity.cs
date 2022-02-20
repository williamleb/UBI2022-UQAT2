using System;
using Fusion;
using Scriptables;
using Systems;
using Systems.Network;
using UnityEngine;

namespace Units.Player
{
    [RequireComponent(typeof(PlayerInteracter))]
    [RequireComponent(typeof(PlayerInputHandler))]
    public partial class PlayerEntity : NetworkBehaviour
    {
        public static event Action<NetworkObject> OnPlayerSpawned;
        
        private PlayerSettings data;
        private PlayerInteracter interacter;
        private NetworkInputData inputs;

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
                Debug.Log("E");
                interacter.InteractWithClosestInteraction();
            }
        }

        private void PlayerLeft(NetworkRunner networkRunner, PlayerRef playerRef)
        {
            if (playerRef == Object.InputAuthority)
                networkRunner.Despawn(Object);
        }
    }
}