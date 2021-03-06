using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;
using Utilities.Extensions;
using Utilities.LayerUtils;
using Utilities.Unity;

namespace Managers.Interactions
{
    public class Interacter : NetworkBehaviour
    {
        private const int NUMBER_OF_COLLIDERS_TO_CHECK = 2;
        private const float UPDATE_RATE = 0.1f;
    
        [SerializeField] private float radius = 5f;
        [SerializeField] private List<SingleUnityLayer> layersToIgnoreCheck = new List<SingleUnityLayer>();

        private readonly List<Interaction> interactionsInReach = new List<Interaction>(NUMBER_OF_COLLIDERS_TO_CHECK);
        private bool isActivated = true;

        public float Radius => radius;

        public int LayerMask => layersToIgnoreCheck.Aggregate(~0, (current, layer) => current & ~layer.Mask);

        public IEnumerable<Interaction> InteractionsInReach => interactionsInReach;
        public bool Activated
        {
            get => isActivated;
            set
            {
                isActivated = value;
                interactionsInReach.Clear();
            }
        }

        public override void Spawned() => InvokeRepeating(nameof(DetectNearbyInteraction),1, UPDATE_RATE);

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

        protected Interaction GetClosestAvailableInteraction()
        {
            if (!interactionsInReach.Any())
                return null;
            
            interactionsInReach.Sort(CompareInteractionDistances);
            return interactionsInReach.FirstOrDefault(interaction => interaction && interaction.CanInteract(this));
        }

        private int CompareInteractionDistances(Interaction left, Interaction right)
        {
            var thisPosition = transform.position;
            var leftPosition = left.transform.position;
            var rightPosition = right.transform.position;
            return VectorExtensions.SqrDistance(leftPosition, thisPosition).CompareTo(VectorExtensions.SqrDistance(rightPosition, thisPosition));
        }

        private readonly Collider[] colliders = new Collider[NUMBER_OF_COLLIDERS_TO_CHECK];
        private void DetectNearbyInteraction()
        {
            if (!Activated)
                return;
         
            interactionsInReach.Clear();
            if (Runner && Runner.GetPhysicsScene().OverlapSphere(transform.position, radius, colliders, Layers.INTERACTION_MASK, QueryTriggerInteraction.UseGlobal) <= 0) return;
            foreach (Collider interact in colliders)
            {
                if (interact && interact.CompareTag(Tags.INTERACTION))
                {
                    if (Vector3.Distance(transform.position, interact.transform.position) <= radius ||
                        interact.bounds.Contains(transform.position))
                        interactionsInReach.Add(interact.GetComponent<Interaction>());   
                }
            }
        }

        private void OnDrawGizmosSelected() => Gizmos.DrawWireSphere(transform.position,radius);
    }
}