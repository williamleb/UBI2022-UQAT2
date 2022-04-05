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

        private void OnTeamRegistered(Team team)
        {
            if (TeamSystem.Instance.Teams.Count != 2)
                return;

            leftTeamStation.associatedTeam = TeamSystem.Instance.Teams[0];
            leftTeamStation.associatedTeam.OnColorChanged += UpdateColor;

            rightTeamStation.associatedTeam = TeamSystem.Instance.Teams[1];
            rightTeamStation.associatedTeam.OnColorChanged += UpdateColor;

            UpdateColor(0);
        }

        void UpdateColor(int _)
        {

            if (leftTeamStation == null || rightTeamStation == null)
            {
                Debug.LogWarning("Missing reference to team station(s). Assigning to a specific team will not work.");
                return;
            }

            if (leftTeamStation.associatedTeam == null || rightTeamStation.associatedTeam == null)
            {
                Debug.LogWarning("Attempted to update the color of the flag, but a reference to one of the two teams is missing. Not updating flag color.");
                return;
            }
            
            leftTeamStation.ChangeFlagColor(SettingsSystem.CustomizationSettings.GetColor(leftTeamStation.associatedTeam.Color));
            rightTeamStation.ChangeFlagColor(SettingsSystem.CustomizationSettings.GetColor(rightTeamStation.associatedTeam.Color));
        }
    }
}