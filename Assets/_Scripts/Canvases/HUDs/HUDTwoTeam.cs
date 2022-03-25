using Canvases.Components;
using Managers.Game;
using Sirenix.OdinInspector;
using Systems.Settings;
using Systems.Teams;
using TMPro;
using UnityEngine;

namespace Canvases.HUDs
{
    public class HUDTwoTeam : MonoBehaviour
    {
        [SerializeField, Required] private TextMeshProUGUI rightTeamName;
        [SerializeField, Required] private ImageUIComponent rightTeamImage;
        [SerializeField, Required] private TextMeshProUGUI rightTeamScore;

        [SerializeField, Required] private TextMeshProUGUI leftTeamName;
        [SerializeField, Required] private ImageUIComponent leftTeamImage;
        [SerializeField, Required] private TextMeshProUGUI leftTeamScore;

        [SerializeField, Required] private SliderUIComponent progressBar;

        private Team leftTeam;
        private Team rightTeam;

        private void Start()
        {
            if (GameManager.HasInstance)
            {
                GameManager.Instance.OnGameStateChanged += UpdateTeamHUD;
            }
        }

        private void UpdateTeamHUD(GameState gameState)
        {
            if (gameState == GameState.Running)
            {
                if (TeamSystem.Instance.Teams.Count != 2)
                {
                    Debug.LogWarning("The game has more than two teams and the UI for two teams is activated. Therefore the UI will not be updated.");
                    return;
                }

                leftTeam = TeamSystem.Instance.Teams[0];
                leftTeam.OnNameChanged += UpdateName;
                leftTeam.OnColorChanged += UpdateColor;
                leftTeam.OnScoreChanged += OnTeamScoreChanged;

                rightTeam = TeamSystem.Instance.Teams[1];
                rightTeam.OnNameChanged += UpdateName;
                rightTeam.OnColorChanged += UpdateColor;
                rightTeam.OnScoreChanged += OnTeamScoreChanged;

                UpdateColor(0);
                UpdateName(null);

                progressBar.backgroundImage.color = SettingsSystem.CustomizationSettings.GetColor(rightTeam.Color);
                progressBar.fillImage.color = SettingsSystem.CustomizationSettings.GetColor(leftTeam.Color);

                Reset();
            }
        }

        void OnTeamScoreChanged(int _)
        {
            if (!rightTeamScore || !leftTeamScore || !rightTeam || !leftTeam)
            {
                Debug.LogWarning("Missing component(s), teams score UI will not be updated.");
                return;
            }

            if (rightTeam.ScoreValue + leftTeam.ScoreValue != 0)
            {
                rightTeamScore.text = rightTeam.ScoreValue.ToString();
                leftTeamScore.text = leftTeam.ScoreValue.ToString();
                progressBar.Value = leftTeam.ScoreValue / (float)(rightTeam.ScoreValue + leftTeam.ScoreValue);
            }
            else
            {
                progressBar.Value = 0.5f;
            }
        }

        private void UpdateName(string _)
        {
            if (!rightTeamName || !leftTeamName || !rightTeam || !leftTeam)
            {
                Debug.LogWarning("Missing component(s), teams name UI will not be updated.");
                return;
            }

            rightTeamName.text = leftTeam.Name;
            leftTeamName.text = rightTeam.Name;
        }

        private void UpdateColor(int _)
        {
            if (!rightTeamImage || !leftTeamImage || !rightTeam || !leftTeam)
            {
                Debug.LogWarning("Missing component(s), teams color UI will not be updated.");
                return;
            }

            rightTeamImage.Color = SettingsSystem.CustomizationSettings.GetColor(leftTeam.Color);
            leftTeamImage.Color = SettingsSystem.CustomizationSettings.GetColor(rightTeam.Color);
        }

        private void Reset()
        {
            rightTeamScore.text = rightTeam.ScoreValue.ToString();
            leftTeamScore.text = leftTeam.ScoreValue.ToString();
            progressBar.Value = 0.5f;
        }

        private void OnDestroy()
        {
            if (leftTeam)
            {
                leftTeam.OnNameChanged -= UpdateName;
                leftTeam.OnColorChanged -= UpdateColor;
                leftTeam.OnScoreChanged -= OnTeamScoreChanged;
            }

            if (rightTeam)
            {
                rightTeam.OnNameChanged -= UpdateName;
                rightTeam.OnColorChanged -= UpdateColor;
                rightTeam.OnScoreChanged -= OnTeamScoreChanged;
            }

            if (GameManager.HasInstance)
                GameManager.Instance.OnGameStateChanged -= UpdateTeamHUD;
        }
    }
}