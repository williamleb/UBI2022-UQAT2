using Managers.Game;
using Sirenix.OdinInspector;
using Systems;
using Systems.Settings;
using TMPro;
using Units.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Canvases.HUDs
{
    public class HUDDashCharge : MonoBehaviour
    {
        [SerializeField, Required] private DashChargeMarker dashChargeMarker;
        [SerializeField, Required] private TextMeshProUGUI dashText;
        [SerializeField, Required] private Image dashButton;

        private Color dashTextColorFull;
        private Color dashTextColorSemi;
        private Color dashButtonColorFull;
        private Color dashButtonColorSemi;

        private PlayerEntity localPlayerEntity;
        private float dashCoolDownTimeInSeconds;
    
        private void Start()
        {
            PlayerSystem.Instance.OnLocalPlayerSpawned += OnLocalPlayerSpawned;
            dashTextColorFull = new Color(dashText.color.r, dashText.color.g, dashText.color.b, 1);
            dashTextColorSemi = new Color(dashText.color.r, dashText.color.g, dashText.color.b, 0.3f);
            dashButtonColorFull = new Color(dashButton.color.r, dashButton.color.g, dashButton.color.b, 1);
            dashButtonColorSemi = new Color(dashButton.color.r, dashButton.color.g, dashButton.color.b, 0.3f);
        }

        private void OnLocalPlayerSpawned(PlayerEntity playerEntity)
        {
            localPlayerEntity = playerEntity;

            if (localPlayerEntity == null)
            {
                Debug.LogWarning("Cannot retrieve local player entity. Not updating dash charge.");
                return;
            }

            localPlayerEntity.OnArchetypeChanged += OnArchetypeChanged;
            localPlayerEntity.OnDashAvailableChanged += OnDashAvailableChanged;
            OnArchetypeChanged();

            Reset();
        }

        private void OnArchetypeChanged()
        {
            if (localPlayerEntity == null)
            {
                Debug.LogWarning("Cannot retrieve local player entity. Not updating dash charge.");
                return;
            }

            dashCoolDownTimeInSeconds = SettingsSystem.Instance.GetPlayerSettings(localPlayerEntity.Archetype).DashCoolDown;
        }

        private void OnDashAvailableChanged(bool isAvailable)
        {
            if (isAvailable)
            {
                dashText.color = dashTextColorFull;
                dashButton.color = dashButtonColorFull;
            }
            else
            {
                dashText.color = dashTextColorSemi;
                dashButton.color = dashButtonColorSemi;
            }
        }

        private void Reset()
        {
            dashCoolDownTimeInSeconds = SettingsSystem.Instance.GetPlayerSettings(localPlayerEntity.Archetype).DashCoolDown;
            dashChargeMarker.ChargeAmount = 1 - (localPlayerEntity.RemainingTimeDashCoolDown / dashCoolDownTimeInSeconds);
        }

        private void Update()
        {
            if (dashChargeMarker == null)
            {
                Debug.LogWarning("Missing dash charge marker reference. Not updating dash charge.");
                return;
            }

            if (localPlayerEntity == null || localPlayerEntity.CanDash)
                return;

            dashChargeMarker.ChargeAmount = 1 - (localPlayerEntity.RemainingTimeDashCoolDown / dashCoolDownTimeInSeconds);
        }

        private void OnDestroy()
        {
            if (PlayerSystem.HasInstance)
                PlayerSystem.Instance.OnLocalPlayerSpawned -= OnLocalPlayerSpawned;

            localPlayerEntity.OnArchetypeChanged -= OnArchetypeChanged;
            localPlayerEntity.OnDashAvailableChanged -= OnDashAvailableChanged;
        }
    }
}
