using Fusion;
using Scriptables;
using Systems;
using Systems.Network;
using UnityEngine;

namespace Units.Player
{
    [RequireComponent(typeof(PlayerInteracter))]
    [RequireComponent(typeof(PlayerInputs))]
    public partial class PlayerEntity : NetworkBehaviour
    {
        private PlayerSettings data;
        private PlayerInteracter interacter;
        private PlayerInputs playerInputs;

        private void Awake()
        {
            data = SettingsSystem.Instance.PlayerSetting;
            
            interacter = GetComponent<PlayerInteracter>();
            playerInputs = GetComponent<PlayerInputs>();
            
            MovementAwake();
        }

        public override void Spawned()
        {
            if (NetworkSystem.Instance.IsPlayer(Object.InputAuthority))
            {
                NetworkSystem.Instance.SetInputFunction(GetInput);
            }
        }
        
        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (NetworkSystem.HasInstance && NetworkSystem.Instance.IsPlayer(Object.InputAuthority))
            {
                NetworkSystem.Instance.UnsetInputFunction();
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (GetInput(out NetworkInputData inputData))
            {
                MoveUpdate(inputData);
                
                if (inputData.Interact)
                {
                    Debug.Log("E");
                    interacter.InteractWithClosestInteraction();
                }
            }
        }
        
        private NetworkInputData GetInput()
        {
            return NetworkInputData.FromPlayerInputs(playerInputs);
        }
    }
}