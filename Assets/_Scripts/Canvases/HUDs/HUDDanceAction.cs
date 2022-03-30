using Sirenix.OdinInspector;
using Systems;
using Systems.Settings;
using TMPro;
using Units.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Canvases.HUDs
{
    public class HUDDanceAction : MonoBehaviour
    {
        [SerializeField, Required] private TextMeshProUGUI danceText;
        [SerializeField, Required] private Image danceButton;

        private PlayerEntity localPlayerEntity;

        private Color danceTextColorFull;
        private Color danceTextColorSemi;
        private Color danceButtonColorFull;
        private Color danceButtonColorSemi;

        private HUDSettings settings;

        void Start()
        {
            settings = SettingsSystem.HUDSettings;

            PlayerSystem.Instance.OnLocalPlayerSpawned += OnLocalPlayerSpawned;

            if (danceText == null || danceButton == null)
                return;

            danceTextColorFull = new Color(danceText.color.r, danceText.color.g, danceText.color.b, 1);
            danceTextColorSemi = new Color(danceText.color.r, danceText.color.g, danceText.color.b, settings.DeactivatedActionOpacity);
            danceButtonColorFull = new Color(danceButton.color.r, danceButton.color.g, danceButton.color.b, 1);
            danceButtonColorSemi = new Color(danceButton.color.r, danceButton.color.g, danceButton.color.b, settings.DeactivatedActionOpacity);
        }

        private void OnLocalPlayerSpawned(PlayerEntity playerEntity)
        {
            localPlayerEntity = playerEntity;

            if (localPlayerEntity == null)
            {
                Debug.LogWarning("Cannot retrieve local player entity. Not updating dash charge.");
                return;
            }
        }

        void Update()
        {
            if (localPlayerEntity == null || danceText == null || danceButton == null)
                return;

            if (localPlayerEntity.CurrentSpeed > 0)
            {
                danceText.color = danceTextColorSemi;
                danceButton.color = danceButtonColorSemi;
            }
            else
            {
                danceText.color = danceTextColorFull;
                danceButton.color = danceButtonColorFull;
            }
        }

        private void OnDestroy()
        {
            if (PlayerSystem.HasInstance)
                PlayerSystem.Instance.OnLocalPlayerSpawned -= OnLocalPlayerSpawned;
        }
    }
}
