using Scriptables;
using Systems;
using UnityEngine;

namespace Units.Player
{
    public partial class PlayerEntity : MonoBehaviour
    {
        [SerializeField] private PlayerInputs playerInputs;
        [SerializeField] private bool immortal;
        private GlobalSettings settings;

        private void Awake()
        {
            settings = SettingsSystem.Instance.Settings;
            MovementAwake();
        }

        private void Start()
        {
            
        }

        private void Update()
        {
            MoveUpdate();
        }

        private void OnDisable()
        {
            
        }

        private void OnTriggerEnter(Collider other)
        {
            
        }

        private void OnTriggerExit(Collider other)
        {
            
        }

    }
}