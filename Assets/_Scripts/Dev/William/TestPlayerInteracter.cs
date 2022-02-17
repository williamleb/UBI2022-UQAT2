using Scriptables;
using Systems;
using Fusion;
using Units.Player;
using UnityEngine;

namespace Dev.William
{
    [RequireComponent(typeof(PlayerInteracter))]
    public class TestPlayerInteracter : NetworkBehaviour
    {
        [SerializeField] private PlayerInputs playerInputs;
        
        private PlayerInteracter interacter;

        private void Awake()
        {
            interacter = GetComponent<PlayerInteracter>();
        }

        public void OnInput()
        {
            
        }
        
        private void Update()
        {
            if (playerInputs.Interact)
            {
                Debug.Log("E");
                interacter.InteractWithClosestInteraction();
            }
        }
    }
}