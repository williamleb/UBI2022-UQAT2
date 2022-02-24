using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;
using Utilities.Extensions;

namespace Managers.Interactions
{
    public class Interacter : NetworkBehaviour
    {
        [SerializeField] private float radius = 5f;
        
        private readonly List<Interaction> interactionsInReach = new List<Interaction>(4);
        public IEnumerable<Interaction> InteractionsInReach => interactionsInReach;

        public override void Spawned() => InvokeRepeating(nameof(DetectNearbyInteraction),1,0.1f);

        public void InteractWithClosestInteraction(bool justStarted = true)
        {
            if (!justStarted)
                return;
            
            var closestInteraction = GetClosestAvailableInteraction();
            if (closestInteraction)
            {
                closestInteraction.Interact(this);
            }
        }

        private Interaction GetInteractionFromId(int interactionId) => interactionsInReach.FirstOrDefault(interaction => interaction.InteractionId == interactionId);

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
            return VectorExtensions.SqrDistance(leftPosition, thisPosition).CompareTo(VectorExtensions.SqrDistance(rightPosition, thisPosition));
        }

        private void DetectNearbyInteraction()
        {
            Collider[] colliders = new Collider[4];
            interactionsInReach.Clear();
            if (Runner.GetPhysicsScene().OverlapSphere(transform.position, radius, colliders, Physics.AllLayers, QueryTriggerInteraction.UseGlobal) <= 0) return;
            foreach (Collider interact in colliders)
            {
                if (!interact)
                    continue;
                
                Debug.Log($"Interact with: {interact.name}");
                if (interact && interact.CompareTag(Interaction.TAG))
                {
                    Debug.Log("Was a thing");
                    interactionsInReach.Add(interact.GetComponent<Interaction>());   
                }
                else
                {
                    Debug.Log("Wasn't a thing");
                }
            }
        }

        private void OnDrawGizmosSelected() => Gizmos.DrawWireSphere(transform.position,radius);
    }
}