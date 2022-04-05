using Managers.Interactions;
using Sirenix.OdinInspector;
using Systems.Sound;
using Systems.Teams;
using Units.Player;
using UnityEngine;

namespace Ingredients.Stations
{
    [RequireComponent(typeof(Interacter))]
    public class TeamStation : MonoBehaviour
    {

        [SerializeField, Required] private Material flagMaterial;

        private Interaction interaction;

        public Team associatedTeam;

        private void Awake()
        {
            interaction = GetComponentInChildren<Interaction>();
            interaction.OnInteractedWith += OnInteractedWith;
            interaction.OnInstantFeedback += OnInstantFeedback;
        }

        private void OnInteractedWith(Interacter interacter)
        {
            var playerEntity = interacter.GetComponent<PlayerEntity>();

            if (associatedTeam == null)
            {
                TeamSystem.Instance.AssignTeam(playerEntity);
            }
            else
            {
                TeamSystem.Instance.AssignTeam(playerEntity,associatedTeam.TeamId);
            }
        }

        private void OnInstantFeedback(Interacter interacter)
        {
            SoundSystem.Instance.PlayInteractWorldElementSound();
        }

        public void ChangeFlagColor(Color color)
        {
            flagMaterial.color = color;
        }
    }
}
