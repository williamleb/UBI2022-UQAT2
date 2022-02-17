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

        protected void OnInteraction()
        {
            OnInteractedWith?.Invoke();
        }

        // Moving the raycast hit outside of the function so we don't create unnecessary garbage as this method can be called frequently
        private RaycastHit hit;
        public bool CanInteract(Interacter interaction)
        {
            if (!Physics.Raycast(transform.position, interaction.transform.position - transform.position, out hit))
                return false;

            if (hit.collider.gameObject != interaction.gameObject)
                return false;
            
            return true;
        }

        public virtual void Interact()
        {
            OnInteraction();
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
    }
}