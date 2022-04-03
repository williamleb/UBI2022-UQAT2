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
    public class HUDThrowAction : MonoBehaviour
    {
        [SerializeField, Required] private TextMeshProUGUI throwText;
        [SerializeField, Required] private Image throwButton;

        private PlayerEntity localPlayerEntity;

        private Color throwTextColorFull;
        private Color throwTextColorSemi;
        private Color throwButtonColorFull;
        private Color throwButtonColorSemi;

        private HUDSettings settings;

        private void Start()
        {
            settings = SettingsSystem.HUDSettings;

            if (GameManager.HasInstance)
                GameManager.Instance.OnGameStateChanged += OnGameStateChanged;

            PlayerSystem.Instance.OnLocalPlayerSpawned += OnLocalPlayerSpawned;

            if (throwText == null || throwButton == null)
                return;

            throwTextColorFull = new Color(throwText.color.r, throwText.color.g, throwText.color.b, 1);
            throwTextColorSemi = new Color(throwText.color.r, throwText.color.g, throwText.color.b,
                settings.DeactivatedActionOpacity);
            throwButtonColorFull = new Color(throwButton.color.r, throwButton.color.g, throwButton.color.b, 1);
            throwButtonColorSemi = new Color(throwButton.color.r, throwButton.color.g, throwButton.color.b,
                settings.DeactivatedActionOpacity);
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
            }
        }

        private void Update()
        {
            if (localPlayerEntity == null || throwText == null || throwButton == null)
                return;

            if (localPlayerEntity.CanThrow)
            {
                throwText.color = throwTextColorFull;
                throwButton.color = throwButtonColorFull;
            }
            else
            {
                throwText.color = throwTextColorSemi;
                throwButton.color = throwButtonColorSemi;
            }
        }

        private void OnDestroy()
        {
            if (PlayerSystem.HasInstance)
                PlayerSystem.Instance.OnLocalPlayerSpawned -= OnLocalPlayerSpawned;
        }
    }
}