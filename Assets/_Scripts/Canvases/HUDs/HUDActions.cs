using Managers.Game;
using Sirenix.OdinInspector;
using Systems;
using Systems.Settings;
using TMPro;
using Units.Player;
using UnityEngine;
using UnityEngine.UI;

public class HUDActions : MonoBehaviour
{
    [Header("Dash action")]
    [SerializeField, Required] private DashChargeMarker dashChargeMarker;
    [SerializeField, Required] private Image dashPrompt;
    [SerializeField, Required] private TextMeshProUGUI dashText;
    private float dashCoolDownTimeInSeconds;

    [Header("Throw action")]
    [SerializeField, Required] private Image throwPrompt;
    [SerializeField, Required] private TextMeshProUGUI throwText;

    [Header("Sprint action")]
    [SerializeField, Required] private Image sprintPrompt;
    [SerializeField, Required] private TextMeshProUGUI sprintText;

    [Header("Dance action")]
    [SerializeField, Required] private Image dancePrompt;
    [SerializeField, Required] private TextMeshProUGUI danceText;

    private Color textColorFull;
    private Color textColorSemi;
    private Color promptColorFull;
    private Color promptColorSemi;

    private PlayerEntity localPlayerEntity;
    private HUDSettings settings;

    void Start()
    {
        settings = SettingsSystem.HUDSettings;

        if (GameManager.HasInstance)
            GameManager.Instance.OnGameStateChanged += OnGameStateChanged;

        PlayerSystem.Instance.OnLocalPlayerSpawned += OnLocalPlayerSpawned;

        textColorFull = new Color(dashText.color.r, dashText.color.g, dashText.color.b, 1);
        textColorSemi = new Color(dashText.color.r, dashText.color.g, dashText.color.b, settings.DeactivatedActionOpacity);
        promptColorFull = new Color(dashPrompt.color.r, dashPrompt.color.g, dashPrompt.color.b, 1);
        promptColorSemi = new Color(dashPrompt.color.r, dashPrompt.color.g, dashPrompt.color.b, settings.DeactivatedActionOpacity);
    }

    private void OnGameStateChanged(GameState gameState)
    {
        if (gameState == GameState.Running)
        {
            SetLocalPlayer(PlayerSystem.Instance.LocalPlayer);
        }
    }

    private void OnLocalPlayerSpawned(PlayerEntity playerEntity)
    {
        if (localPlayerEntity == null)
        {
            SetLocalPlayer(playerEntity);
        }
    }

    private void SetLocalPlayer(PlayerEntity playerEntity)
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
            dashChargeMarker.gameObject.SetActive(false);
            dashText.color = textColorFull;
            dashPrompt.color = promptColorFull;
        }
        else
        {
            dashChargeMarker.gameObject.SetActive(true);
            dashText.color = textColorSemi;
            dashPrompt.color = promptColorSemi;
        }
    }

    private void Reset()
    {
        dashChargeMarker.gameObject.SetActive(false);
        dashCoolDownTimeInSeconds = SettingsSystem.Instance.GetPlayerSettings(localPlayerEntity.Archetype).DashCoolDown;
        dashChargeMarker.ChargeAmount = 1;
    }

    void Update()
    {
        DashActionUpdate();
        ThrowActionUpdate();
        SprintActionUpdate();
        DanceActionUpdate();
    }

    void DashActionUpdate()
    {
        if (dashChargeMarker == null)
        {
            Debug.LogWarning("Missing HUD Dash action components. NOT updating Dash action HUD...");
            return;
        }

        if (localPlayerEntity == null || localPlayerEntity.CanDash)
            return;

        dashChargeMarker.ChargeAmount = 1 - (localPlayerEntity.RemainingTimeDashCoolDown / dashCoolDownTimeInSeconds);
    }

    void ThrowActionUpdate()
    {
        if (throwText == null || throwPrompt == null)
        {
            Debug.LogWarning("Missing HUD Throw action components. NOT updating Throw action HUD...");
            return;
        }

        if (localPlayerEntity == null)
            return;

        if (localPlayerEntity.CanThrow)
        {
            throwText.color = textColorFull;
            throwPrompt.color = promptColorFull;
        }
        else
        {
            throwText.color = textColorSemi;
            throwPrompt.color = promptColorSemi;
        }
    }

    void SprintActionUpdate()
    {
        //Waiting for Martin rework on playerentity before updating sprint action.
    }

    void DanceActionUpdate()
    {
        if (localPlayerEntity == null || danceText == null || dancePrompt == null)
        {
            Debug.LogWarning("Missing HUD Dance action components. NOT updating Dance action HUD...");
            return;
        }   

        if (localPlayerEntity.CurrentSpeed > 0)
        {
            danceText.color = textColorSemi;
            dancePrompt.color = promptColorSemi;
        }
        else
        {
            danceText.color = textColorFull;
            dancePrompt.color = promptColorFull;
        }
    }

    private void OnDestroy()
    {
        if (PlayerSystem.HasInstance)
            PlayerSystem.Instance.OnLocalPlayerSpawned -= OnLocalPlayerSpawned;

        if (GameManager.HasInstance)
            GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;

        if (localPlayerEntity != null)
        {
            localPlayerEntity.OnArchetypeChanged -= OnArchetypeChanged;
            localPlayerEntity.OnDashAvailableChanged -= OnDashAvailableChanged;
        }
    }
}
