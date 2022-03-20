using Canvases.Components;
using Managers.Score;
using Sirenix.OdinInspector;
using Systems.Teams;
using UnityEngine;

namespace Canvases.HUDs
{
    public class HUDScore : MonoBehaviour
    {
        [SerializeField, Required] private TextUIComponent scoreText;
        [SerializeField, Required] private TextUIComponent teamName;
        
        private Team team;

        public void Init(Team teamToLookAt)
        {
            Debug.Assert(teamToLookAt);
            team = teamToLookAt;

            scoreText.Text = $"{team.ScoreValue}";
            teamName.Text = $"{team.Name}";

            team.OnScoreChanged += OnScoreChanged;
        }

        private void OnDestroy()
        {
            if (team)
                team.OnScoreChanged -= OnScoreChanged;
        }

        private void OnScoreChanged(int newScore)
        {
            // TODO Kool animation
            scoreText.Text = $"{newScore}";
        }
    }
}