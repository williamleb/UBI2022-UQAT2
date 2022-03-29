using Canvases.Components;
using Managers.Game;
using Sirenix.OdinInspector;
using Systems.Settings;
using Systems.Teams;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Canvases.HUDs
{
    public class HUDTwoTeam : MonoBehaviour
    {
        [Header("Left team")]
        [SerializeField, Required] private TextMeshProUGUI leftTeamName;
        [SerializeField, Required] private SliderUIComponent leftTeamScoreSlider;
        [SerializeField, Required] private TextMeshProUGUI leftTeamScore;
        [SerializeField, Required] private Image leftScratch;

        [Header("Right team")]
        [SerializeField, Required] private TextMeshProUGUI rightTeamName;
        [SerializeField, Required] private SliderUIComponent rightTeamScoreSlider;
        [SerializeField, Required] private TextMeshProUGUI rightTeamScore;
        [SerializeField, Required] private Image rightScratch;

        [Space]
        [Header("Other settings")]
        [SerializeField, Required] [Range(0f,1f)] private float scratchAlpha;

        private Team leftTeam;
        private Team rightTeam;

        private void Start()
        {
            if (GameManager.HasInstance)
            {
                GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
            }
        }

        private void OnGameStateChanged(GameState gameState)
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

                Reset();
            }
        }

        void OnTeamScoreChanged(int _)
        {
            if (!rightTeamScoreSlider || !leftTeamScoreSlider || !rightTeamScore || !leftTeamScore || !rightTeam || !leftTeam)
            {
                Debug.LogWarning("Missing component(s), teams score UI will not be updated.");
                return;
            }

            rightTeamScore.text = rightTeam.ScoreValue.ToString();
            leftTeamScore.text = leftTeam.ScoreValue.ToString();

            rightTeamScoreSlider.Value = rightTeam.ScoreValue / (float)SettingsSystem.GameSettings.NumberOfHomeworksToFinishGame;
            leftTeamScoreSlider.Value = leftTeam.ScoreValue / (float)SettingsSystem.GameSettings.NumberOfHomeworksToFinishGame;
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

        private void UpdateColor(int _)
        {
            if (!rightTeamScoreSlider || !leftTeamScoreSlider || !rightTeamName || !leftTeamName || !rightTeamScore || !leftTeamScore || !rightTeam || !leftTeam)
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

            rightTeamScoreSlider.FillColor = rightTeamColorBoostValue;
            rightTeamScoreSlider.FrameColor = Color.HSVToRGB(rH, rS, 0.5f);
            rightTeamScoreSlider.BackgroundColor = Color.HSVToRGB(rH, rS, 0.2f);

            leftTeamScoreSlider.FillColor = leftTeamColorBoostValue;
            leftTeamScoreSlider.FrameColor = Color.HSVToRGB(lH, lS, 0.5f);
            leftTeamScoreSlider.BackgroundColor = Color.HSVToRGB(lH, lS, 0.2f);

            rightTeamName.color = rightTeamColorBoostValue;
            leftTeamName.color = leftTeamColorBoostValue;

            rightTeamScore.color = rightTeamColorBoostValue;
            leftTeamScore.color = leftTeamColorBoostValue;

            rightScratch.color = new Color(rightTeamColor.r, rightTeamColor.g, rightTeamColor.b, scratchAlpha);
            leftScratch.color = new Color(leftTeamColor.r, leftTeamColor.g, leftTeamColor.b, scratchAlpha);
        }

        private void Reset()
        {
            rightTeamScore.text = rightTeam.ScoreValue.ToString();
            leftTeamScore.text = leftTeam.ScoreValue.ToString();
            rightTeamScoreSlider.Value = rightTeam.ScoreValue / (float)SettingsSystem.GameSettings.NumberOfHomeworksToFinishGame;
            leftTeamScoreSlider.Value = leftTeam.ScoreValue / (float)SettingsSystem.GameSettings.NumberOfHomeworksToFinishGame;
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
                GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
        }
    }
}