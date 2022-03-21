using Managers.Interactions;
using Systems.Teams;
using Units.Player;
using UnityEngine;

public class TeamStation : MonoBehaviour
{
    private Interaction interaction;

    private void Awake()
    {
        interaction = GetComponent<Interaction>();
        interaction.OnInteractedWith += OnInteractedWith;
    }

    private void OnInteractedWith(Interacter interacter)
    {
        var playerEntity = interacter.GetComponent<PlayerEntity>();
        TeamSystem.Instance.AssignTeam(playerEntity);
    }
}
