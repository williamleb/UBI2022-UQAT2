using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;

namespace Managers.Interactions
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class Interacter : NetworkBehaviour
    {
        private readonly List<Interaction> interactionsInReach = new List<Interaction>();

        protected IEnumerable<Interaction> InteractionsInReach => interactionsInReach;

        public void InteractWithClosestInteraction()
        {
            var closestInteraction = GetClosestAvailableInteraction();
            if (closestInteraction)
            {
                closestInteraction.Interact();
            }
        }

        private Interaction GetInteractionFromId(int interactionId)
        {
            return interactionsInReach.FirstOrDefault(interaction => interaction.InteractionId == interactionId);
        }

        protected Interaction GetClosestAvailableInteraction()
        {
            if (!interactionsInReach.Any())
                return null;
            
            interactionsInReach.Sort(CompareInteractionDistances);
            return interactionsInReach.FirstOrDefault(interaction => interaction.CanInteract(this));
        }

        private int CompareInteractionDistances(Interaction left, Interaction right)
        {
            var thisPosition = transform.position;
            var leftPosition = left.transform.position;
            var rightPosition = right.transform.position;
            
            return Vector3.Distance(leftPosition, thisPosition).CompareTo(Vector3.Distance(rightPosition, thisPosition));
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(Interaction.TAG)) 
                return;
            
            var interaction = other.GetComponent<Interaction>();
            Debug.Assert(interaction, $"Objects with the tag {Interaction.TAG} must have a {nameof(Interaction)} component");
            interactionsInReach.Add(interaction);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag(Interaction.TAG)) 
                return;
            
            var interaction = other.GetComponent<Interaction>();
            Debug.Assert(interaction, $"Objects with the tag {Interaction.TAG} must have a {nameof(Interaction)} component");
            interactionsInReach.Remove(interaction);
        }
    }
}