using Fusion;
using Managers.Game;
using Sirenix.OdinInspector;
using Systems;
using Systems.Settings;
using TMPro;
using Units.Player;
using UnityEngine;
using UnityEngine.UI;

public class HUDDashCharge : MonoBehaviour
{
    [SerializeField, Required] private DashChargeMarker dashChargeMarker;
    [SerializeField, Required] private TextMeshProUGUI dashtText;
    [SerializeField, Required] private Image dashButton;

    private Color dashTextColorFull;
    private Color dashTextColorSemi;
    private Color dashButtonColorFull;
    private Color dashButtonColorSemi;

    private PlayerEntity localPlayerEntity;
    private float dashCoolDownTimeInSeconds;

    [SerializeField, Required] 
    void Start()
    {
        if (GameManager.HasInstance)
        {
            GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
        }

        dashTextColorFull = new Color(dashtText.color.r, dashtText.color.g, dashtText.color.b, 1);
        dashTextColorSemi = new Color(dashtText.color.r, dashtText.color.g, dashtText.color.b, 0.3f);
        dashButtonColorFull = new Color(dashButton.color.r, dashButton.color.g, dashButton.color.b, 1);
        dashButtonColorSemi = new Color(dashButton.color.r, dashButton.color.g, dashButton.color.b, 0.3f);
    }

    private void OnGameStateChanged(GameState gameState)
    {
        if (gameState == GameState.Running)
        {
            localPlayerEntity = PlayerSystem.Instance.LocalPlayer;

            if (localPlayerEntity == null)
            {
                Debug.LogWarning("Cannot retreive local player entity. Not updating dash charge.");
                return;
            }

            localPlayerEntity.OnArchetypeChanged += OnArchetypeChanged;
            localPlayerEntity.OnDashAvailableChanged += OnDashAvailableChanged;
            OnArchetypeChanged();

            Reset();
        }
    }

    private void OnArchetypeChanged()
    {
        if (localPlayerEntity == null)
        {
            Debug.LogWarning("Cannot retreive local player entity. Not updating dash charge.");
            return;
        }

        dashCoolDownTimeInSeconds = SettingsSystem.Instance.GetPlayerSettings(localPlayerEntity.Archetype).DashCoolDown;
    }

    private void OnDashAvailableChanged(bool isAvailable)
    {
        if (isAvailable)
        {
            dashtText.color = dashTextColorFull;
            dashButton.color = dashButtonColorFull;
        }
        else
        {
            dashtText.color = dashTextColorSemi;
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
        if (GameManager.HasInstance)
            GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
    }
}
