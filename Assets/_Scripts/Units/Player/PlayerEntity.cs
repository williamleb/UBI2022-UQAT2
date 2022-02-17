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

        private void Update()
        {
            MoveUpdate();
        }
    }
}