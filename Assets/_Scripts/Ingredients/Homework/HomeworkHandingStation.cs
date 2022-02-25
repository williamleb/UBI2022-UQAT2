﻿using Managers.Game;
using Managers.Interactions;
using Units.Player;
using UnityEngine;
using Utilities.Extensions;
using Utilities.Unity;

namespace Units.AI
{
    [RequireComponent(typeof(Interaction))]
    public class HomeworkHandingStation : MonoBehaviour
    {
        private Interaction giveHomeworkInteraction;

        private void Awake()
        {
            giveHomeworkInteraction = GetComponent<Interaction>();
        }

        private void OnEnable()
        {
            giveHomeworkInteraction.AddValidator(CanGiveHomework);
            giveHomeworkInteraction.OnInstantFeedback += OnGiveHomeworkInstantFeedback;
            giveHomeworkInteraction.OnInteractedWith += OnGiveHomework;
        }

        private void OnDisable()
        {
            giveHomeworkInteraction.RemoveValidator(CanGiveHomework);
            giveHomeworkInteraction.OnInstantFeedback -= OnGiveHomeworkInstantFeedback;
            giveHomeworkInteraction.OnInteractedWith -= OnGiveHomework;
        }

        private bool CanGiveHomework(Interacter interacter)
        {
            var interacterGameObject = interacter.gameObject;
            if (!interacterGameObject.CompareTag(Tags.PLAYER) && !interacterGameObject.CompareTag(Tags.AI))
                return false;

            var inventory = interacterGameObject.GetComponentInEntity<Inventory>();
            if (!inventory)
                return false;

            if (!inventory.HasHomework)
                return false;
            
            return true;
        }

        private void OnGiveHomeworkInstantFeedback(Interacter interacter)
        {
            // TODO Play instant feedback on give homework
        }

        private void OnGiveHomework(Interacter interacter)
        {
            var inventory = interacter.gameObject.GetComponentInEntity<Inventory>();
            Debug.Assert(inventory, $"{nameof(HomeworkHandingStation)} should only be interacted with by actors with an {nameof(Inventory)}");

            if (interacter.gameObject.CompareTag(Tags.PLAYER))
                HandHomework(interacter);
            
            inventory.RemoveHomework();
        }

        private void HandHomework(Interacter interacter)
        {
            if (!GameManager.HasInstance)
                return;
            
            var player = interacter.gameObject.GetComponentInEntity<PlayerEntity>();
            Debug.Assert(player, $"An interacter with the tag {Tags.PLAYER} should have a {nameof(PlayerEntity)}");
            GameManager.Instance.HandHomework(player);
        }
    }
}