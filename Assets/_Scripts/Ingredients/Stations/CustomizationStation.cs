using System;
using Managers.Interactions;
using Units.Player;
using UnityEngine;
using Utilities.Extensions;

namespace Ingredients.Stations
{
    [RequireComponent(typeof(Interaction))]
    public class CustomizationStation : MonoBehaviour
    {
        private Interaction interaction;

        private void Awake()
        {
            interaction = GetComponent<Interaction>();
        }

        private void Start()
        {
            interaction.OnInstantFeedback += StartCustomization;
        }

        private void StartCustomization(Interacter interacter)
        {
            if (!interacter.gameObject.IsAPlayer())
                return;

            var player = interacter.gameObject.GetComponentInEntity<PlayerEntity>();
            if (!player)
                return;
            
            player.StartCustomization();
        }
    }
}