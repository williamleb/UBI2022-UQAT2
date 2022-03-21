using Canvases.Components;
using Systems.Teams;
using UnityEngine;

namespace Canvases.Menu.Customization
{
    public class CustomizationUIRandomizeTeamName : CustomizationUIButton
    {
        [SerializeField] private TextUIComponent teamName;

        private Team team;

        protected override void Init()
        {
            base.Init();
            UnsubscribeToPreviousTeam();
            team = TeamSystem.Instance.GetTeam(CustomizablePlayer.TeamId);
            SubscribeToTeam();

            if (!team)
            {
                Debug.LogWarning($"No team for player {CustomizablePlayer.PlayerId} found");
                return;
            }
            
            UpdateTeamNameText();
        }

        private void UnsubscribeToPreviousTeam()
        {
            if (!team)
                return;

            team.OnNameChanged -= OnTeamNameChanged;
        }

        private void SubscribeToTeam()
        {
            if (!team)
                return;

            team.OnNameChanged += OnTeamNameChanged;
        }

        private void OnDestroy()
        {
            UnsubscribeToPreviousTeam();
        }

        protected override void OnClick()
        {
            if (!team)
                return;
            
            team.RandomizeName();
        }

        private void OnTeamNameChanged(string newName)
        {
            UpdateTeamNameText();
        }

        private void UpdateTeamNameText()
        {
            if (!teamName || !team)
                return;
            
            teamName.Text = team.Name;
        }
    }
}