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
        private List<Interaction> interactionsInReach = new List<Interaction>();

        // TODO PlayerInteracter: grab only nearest + show prompt if active player
        // TODO AIInteracter: grab whatever (pass an ID)

        public List<Interaction> InteractionsInReach => interactionsInReach;

        public void InteractWithClosestInteraction()
        {
            // TODO instant feedback while still managing timed interactions
            var closestInteraction = GetClosestAvailableInteraction();
            if (closestInteraction)
            {
                RPC_Interact(closestInteraction.InteractionId);
            }
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, InvokeLocal = false)]
        private void RPC_Interact(int interactionId)
        {
            Debug.Log("Interacter.RPC_Interact() =============================="); // TODO Remove

            var interaction = GetInteractionFromId(interactionId);
            if (!interaction)
            {
                // The host client didn't have the requested interaction in the interactions in reach for this entity
                // For now, we only fallback to the interaction manager without checking if the interaction was disabled because another player interacted with it or if the player could interact with it (if they are cheating)
                // This might change in the future
                if (InteractionManager.HasInstance)
                    interaction = InteractionManager.Instance.GetInteraction(interactionId);
            }
            
            if (interaction)
                interaction.Interact();
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