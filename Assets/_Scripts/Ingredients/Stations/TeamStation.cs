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
        
        public Team AssociatedTeam { get; set; }
        private Color Color => flagMaterial.color;

        private void Awake()
        {
            interaction = GetComponentInChildren<Interaction>();
            interaction.OnInteractedWith += OnInteractedWith;
            interaction.OnInstantFeedback += OnInstantFeedback;
        }

        private void OnInteractedWith(Interacter interacter)
        {
            var playerEntity = interacter.GetComponent<PlayerEntity>();
            Debug.Assert(playerEntity);

            if (AssociatedTeam == null)
            {
                TeamSystem.Instance.AssignTeam(playerEntity);
            }
            else
            {
                TeamSystem.Instance.AssignTeam(playerEntity, AssociatedTeam.TeamId);
            }
            
            playerEntity.PlayTeamSwapFXOnOtherClients(Color);
        }

        private void OnInstantFeedback(Interacter interacter)
        {
            var playerEntity = interacter.GetComponent<PlayerEntity>();
            Debug.Assert(playerEntity);
            
            SoundSystem.Instance.PlayInteractWorldElementSound();
            playerEntity.PlayTeamSwapFXLocally(Color);
        }

        public void ChangeFlagColor(Color color)
        {
            flagMaterial.color = color;
        }
    }
}
