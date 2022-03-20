using Canvases.Components;
using Sirenix.OdinInspector;
using Systems.Settings;
using Systems.Teams;
using UnityEngine;

namespace Canvases.HUDs
{
    public class HUDScore : MonoBehaviour
    {
        [SerializeField, Required] private TextUIComponent scoreText;
        [SerializeField, Required] private TextUIComponent teamName;
        [SerializeField] private ImageUIComponent teamColorImage;
        
        private Team team;

        public void Init(Team teamToLookAt)
        {
            Debug.Assert(teamToLookAt);
            team = teamToLookAt;

            UpdateScore(team.ScoreValue);
            UpdateName(team.Name);
            UpdateColor(team.Color);
            
            team.OnScoreChanged += UpdateScore;
            team.OnNameChanged += UpdateName;
            team.OnColorChanged += UpdateColor;
        }

        private void OnDestroy()
        {
            if (team)
                team.OnScoreChanged -= UpdateScore;
        }

        private void UpdateScore(int newScore)
        {
            // TODO Kool animation
            scoreText.Text = $"{newScore}";
        }

        private void UpdateName(string newName)
        {
            teamName.Text = newName;
        }

        private void UpdateColor(int color)
        {
            if (!teamColorImage)
                return;
            
            teamColorImage.Color = SettingsSystem.CustomizationSettings.GetColor(color);;
        }
    }
}