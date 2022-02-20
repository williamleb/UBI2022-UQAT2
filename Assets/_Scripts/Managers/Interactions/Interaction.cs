using System;
using Canvases.Markers;
using Fusion;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Managers.Interactions
{
    [RequireComponent(typeof(Collider))]
    [ValidateInput(nameof(ValidateIfHasTag), "An interaction component must be placed on a collider that has the 'Interaction' tag.")]
    [ValidateInput(nameof(ValidateIfColliderIsTrigger), "An interaction component must have a collider of type 'trigger'.")]
    public class Interaction : NetworkBehaviour
    {
        public const string TAG = "Interaction";
        
        public event Action OnInteractedWith;
        public event Action OnInstantFeedback;
        
        [SerializeField] private SpriteMarkerReceptor markerToShowWhenInteractionPossible;

        private bool interactionPossible = false;

        public bool Possible
        {
            get => interactionPossible;
            set
            {
                if (interactionPossible == value)
                    return;
                
                interactionPossible = value;
                
                if (!markerToShowWhenInteractionPossible)
                    return;
                
                if (interactionPossible)
                    markerToShowWhenInteractionPossible.Activate();
                else
                    markerToShowWhenInteractionPossible.Deactivate();
            }
        }

        public int InteractionId => Id.GetHashCode();

        public override void Spawned()
        {
            if (InteractionManager.HasInstance)
            {
                InteractionManager.Instance.RegisterInteraction(this);
            }
        }
        
        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (InteractionManager.HasInstance)
            {
                InteractionManager.Instance.UnregisterInteraction(this);
            }
        }

        // Moving the raycast hit outside of the function so we don't create unnecessary garbage as this method can be called frequently
        private RaycastHit hit;
        public bool CanInteract(Interacter interacter)
        {
            if (!Physics.Raycast(transform.position, interacter.transform.position - transform.position, out hit))
                return false;

            if (hit.collider.gameObject != interacter.gameObject)
                return false;
            
            return true;
        }

        public void Interact(Interacter interacter)
        {
            if (!CanInteract(interacter))
                return;
            
            if (interacter.Object.HasInputAuthority)
                OnInstantFeedback?.Invoke();
            
            RPC_Interact();
        }
        
        private bool ValidateIfHasTag()
        {
            return gameObject.CompareTag(TAG);
        }

        private bool ValidateIfColliderIsTrigger()
        {
            var colliders = GetComponents<Collider>();
            foreach (var colliderComponent in colliders)
            {
                if (colliderComponent.isTrigger)
                {
                    return true;
                }
            }

            return false;
        }
        
        protected void RPC_Interact()
        {
            OnInteractedWith?.Invoke();
        }
    }
}