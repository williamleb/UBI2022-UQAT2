using Managers.Game;
using Sirenix.OdinInspector;
using Systems.Settings;
using Systems.Teams;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Canvases.HUDs
{
    public class HUDTeamNameLobby : MonoBehaviour
    {
        [Header("Left team")]
        [SerializeField, Required] private TextMeshProUGUI leftTeamName;
        [SerializeField, Required] private TextMeshProUGUI leftTeamPlayerCount;
        [SerializeField, Required] private Image leftScratch;

        [Header("Right team")]
        [SerializeField, Required] private TextMeshProUGUI rightTeamName;
        [SerializeField, Required] private TextMeshProUGUI rightTeamPlayerCount;
        [SerializeField, Required] private Image rightScratch;

        [Space]
        [Header("Other settings")]
        [SerializeField, Required] [Range(0f,1f)] private float scratchAlpha;

        private Team leftTeam;
        private Team rightTeam;

        private void Start()
        {
            if (TeamSystem.HasInstance && TeamSystem.Instance.AreTeamsCreated)
            {
                OnTeamRegistered(null);
            }
            else
            {
                TeamSystem.OnTeamRegistered += OnTeamRegistered;
            }
        }

        private void OnTeamRegistered(Team team)
        {
            if (TeamSystem.Instance.Teams.Count != 2)
                return;

            leftTeam = TeamSystem.Instance.Teams[0];
            leftTeam.OnNameChanged += UpdateName;
            leftTeam.OnPlayerCountChanged += PlayerCountChanged;
            leftTeam.OnColorChanged += UpdateColor;

            rightTeam = TeamSystem.Instance.Teams[1];
            rightTeam.OnNameChanged += UpdateName;
            rightTeam.OnPlayerCountChanged += PlayerCountChanged;
            rightTeam.OnColorChanged += UpdateColor;

            UpdateColor(0);
            UpdateName(null);
            PlayerCountChanged(0);
        }

        private void PlayerCountChanged(int _)
        {
            if (!leftTeamPlayerCount || !rightTeamPlayerCount || !rightTeam || !leftTeam)
            {
                Debug.LogWarning("Missing component(s), teams name UI will not be updated.");
                return;
            }
            
            rightTeamPlayerCount.text = rightTeam.PlayerCount.ToString() + (rightTeam.PlayerCount > 1 ? " players" : " player");
            leftTeamPlayerCount.text = leftTeam.PlayerCount.ToString() + (leftTeam.PlayerCount > 1 ? " players" : " player");
        }

        private void UpdateColor(int _)
        {
            if (!rightTeamName || !leftTeamName || !rightTeam || !leftTeam)
            {
                Debug.LogWarning("Missing component(s), teams color UI will not be updated.");
                return;
            }

            Color rightTeamColor = SettingsSystem.CustomizationSettings.GetColor(rightTeam.Color);
            Color leftTeamColor = SettingsSystem.CustomizationSettings.GetColor(leftTeam.Color);
            
            //Bost value for better contrast
            Color.RGBToHSV(rightTeamColor, out float rH, out float rS, out float _);
            Color.RGBToHSV(leftTeamColor, out float lH, out float lS, out float _);
            Color rightTeamColorBoostValue = Color.HSVToRGB(rH, rS, 1);
            Color leftTeamColorBoostValue = Color.HSVToRGB(lH, lS, 1);

            rightTeamName.color = rightTeamColorBoostValue;
            leftTeamName.color = leftTeamColorBoostValue;

            rightScratch.color = new Color(rightTeamColor.r, rightTeamColor.g, rightTeamColor.b, scratchAlpha);
            leftScratch.color = new Color(leftTeamColor.r, leftTeamColor.g, leftTeamColor.b, scratchAlpha);
        }

        private void UpdateName(string _)
        {

            if (!rightTeamName || !leftTeamName || !rightTeam || !leftTeam)
            {
                Debug.LogWarning("Missing component(s), teams name UI will not be updated.");
                return;
            }

            rightTeamName.text = rightTeam.Name;
            leftTeamName.text = leftTeam.Name;
        }

        private void OnDestroy()
        {
            if (leftTeam)
            {
                leftTeam.OnNameChanged -= UpdateName;
                leftTeam.OnPlayerCountChanged -= PlayerCountChanged;
                leftTeam.OnColorChanged -= UpdateColor;
            }

            if (rightTeam)
            {
                rightTeam.OnNameChanged -= UpdateName;
                rightTeam.OnPlayerCountChanged -= PlayerCountChanged;
                rightTeam.OnColorChanged -= UpdateColor;
            }

            TeamSystem.OnTeamRegistered -= OnTeamRegistered;
        }
    }
}