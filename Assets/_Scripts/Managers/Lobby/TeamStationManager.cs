using Ingredients.Stations;
using Sirenix.OdinInspector;
using Systems.Settings;
using Systems.Teams;
using UnityEngine;

namespace Managers.Lobby
{ 
    public class TeamStationManager : MonoBehaviour
    {
        [SerializeField, Required] private TeamStation leftTeamStation;
        [SerializeField, Required] private TeamStation rightTeamStation;

        void Start()
        {
            if(leftTeamStation == null || rightTeamStation == null)
            {
                Debug.LogWarning("Missing reference to team station(s). Assigning to a specific team will not work.");
                return;
            }

            if (TeamSystem.HasInstance && TeamSystem.Instance.AreTeamsCreated)
            {
                OnTeamRegistered(null);
            }
            else
            {
                TeamSystem.OnTeamRegistered += OnTeamRegistered;
            }
        }

        private void OnDestroy()
        {
            TeamSystem.OnTeamRegistered -= OnTeamRegistered;
        }

        private void OnTeamRegistered(Team team)
        {
            if (TeamSystem.Instance.Teams.Count != 2)
                return;

            leftTeamStation.AssociatedTeam = TeamSystem.Instance.Teams[0];
            leftTeamStation.AssociatedTeam.OnColorChanged += UpdateColor;

            rightTeamStation.AssociatedTeam = TeamSystem.Instance.Teams[1];
            rightTeamStation.AssociatedTeam.OnColorChanged += UpdateColor;

            UpdateColor(0);
        }

        void UpdateColor(int _)
        {

            if (leftTeamStation == null || rightTeamStation == null)
            {
                Debug.LogWarning("Missing reference to team station(s). Assigning to a specific team will not work.");
                return;
            }

            if (leftTeamStation.AssociatedTeam == null || rightTeamStation.AssociatedTeam == null)
            {
                Debug.LogWarning("Attempted to update the color of the flag, but a reference to one of the two teams is missing. Not updating flag color.");
                return;
            }
            
            leftTeamStation.ChangeFlagColor(SettingsSystem.CustomizationSettings.GetColor(leftTeamStation.AssociatedTeam.Color));
            rightTeamStation.ChangeFlagColor(SettingsSystem.CustomizationSettings.GetColor(rightTeamStation.AssociatedTeam.Color));
        }
    }
}