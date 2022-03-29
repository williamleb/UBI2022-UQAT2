using Canvases.Markers;
using Managers.Interactions;
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
        GetComponentInChildren<TextMarkerReceptor>().Text = $"Select\n{archetypeTypeStation}";
    }

    private void OnInteractedWith(Interacter interacter)
    {
        var playerEntity = interacter.GetComponent<PlayerEntity>();
        playerEntity.AssignArchetype(archetypeTypeStation);
    }
}
