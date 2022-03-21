using Canvases.Components;
using Systems.Settings;
using Systems.Teams;
using UnityEngine;

namespace Canvases.Menu.Customization
{
    public class CustomizationUITeamColor : CustomizationUIUpDown
    {
        [SerializeField] private ImageUIComponent teamImage;

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
            
            UpdateTeamImageColor();
        }

        private void UnsubscribeToPreviousTeam()
        {
            if (!team)
                return;

            team.OnColorChanged -= OnTeamColorChanged;
        }

        private void SubscribeToTeam()
        {
            if (!team)
                return;

            team.OnColorChanged += OnTeamColorChanged;
        }

        private void OnDestroy()
        {
            UnsubscribeToPreviousTeam();
        }

        protected override void OnUp()
        {
            if (!team)
                return;
            
            team.IncrementColor();
        }

        protected override void OnDown()
        {
            if (!team)
                return;
            
            team.DecrementColor();
        }
        
        private void OnTeamColorChanged(int newColor)
        {
            UpdateTeamImageColor();
        }

        private void UpdateTeamImageColor()
        {
            if (!teamImage || !team)
                return;

            var color = SettingsSystem.CustomizationSettings.GetColor(team.Color);
            teamImage.Color = color;
        }
    }
}