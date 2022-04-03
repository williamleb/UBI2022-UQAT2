using Canvases.Markers;
using Managers.Interactions;
using Systems.Sound;
using Units.Player;
using UnityEngine;

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
        SoundSystem.Instance.PlayCharacterSelectSound(archetypeTypeStation);
    }

    private void OnInteractedWith(Interacter interacter)
    {
        var playerEntity = interacter.GetComponent<PlayerEntity>();
        playerEntity.AssignArchetype(archetypeTypeStation);
    }
}
