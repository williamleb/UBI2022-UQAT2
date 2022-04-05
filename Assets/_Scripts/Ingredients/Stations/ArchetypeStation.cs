using Canvases.Markers;
using Managers.Interactions;
using Systems.Sound;
using Units.Player;
using UnityEngine;

namespace Ingredients.Stations
{
    public class ArchetypeStation : MonoBehaviour
    {
        private Interaction interaction;
        [SerializeField] private Archetype archetypeTypeStation;

        private void Awake()
        {
            interaction = GetComponent<Interaction>();
            interaction.OnInteractedWith += OnInteractedWith;
            interaction.OnInstantFeedback += OnInstantFeedback;
        }

        private void OnDestroy()
        {
            interaction.OnInteractedWith -= OnInteractedWith;
            interaction.OnInstantFeedback -= OnInstantFeedback;
        }

        private void OnInstantFeedback(Interacter interacter)
        {
            var playerEntity = interacter.GetComponent<PlayerEntity>();
            Debug.Assert(playerEntity);
            
            SoundSystem.Instance.PlayCharacterSelectSound(archetypeTypeStation);
            playerEntity.PlayArchetypeSwapFXLocally(archetypeTypeStation);
        }

        private void OnInteractedWith(Interacter interacter)
        {
            var playerEntity = interacter.GetComponent<PlayerEntity>();
            Debug.Assert(playerEntity);

            playerEntity.AssignArchetype(archetypeTypeStation);
            playerEntity.PlayArchetypeSwapFXOnOtherClients(archetypeTypeStation);
        }
    }
}
