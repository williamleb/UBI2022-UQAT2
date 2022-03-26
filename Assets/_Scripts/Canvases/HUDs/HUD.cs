using Sirenix.OdinInspector;
using Systems.Settings;
using UnityEngine;

namespace Canvases.HUDs
{
    public class HUD : MonoBehaviour
    {
        [SerializeField, Required] private GameObject hudTwoTeamsContainer;
        [SerializeField, Required] private GameObject hudMultipleTeamsContainer;

        private void Start()
        {
            if (SettingsSystem.TeamSettings.NumberOfTeam > 2)
            {
                hudTwoTeamsContainer.SetActive(false);
                hudMultipleTeamsContainer.SetActive(true);
            }
            else
            {
                hudTwoTeamsContainer.SetActive(true);
                hudMultipleTeamsContainer.SetActive(false);
            }
        }
    }
}