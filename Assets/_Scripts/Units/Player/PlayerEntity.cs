using Scriptables;
using Systems;
using UnityEngine;

namespace Units.Player
{
    public partial class PlayerEntity : MonoBehaviour
    {
        [SerializeField] private PlayerInputs playerInputs;
        private PlayerSettings data;

        private void Awake()
        {
            data = SettingsSystem.Instance.PlayerSetting;
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