using System;
using Canvases.Markers;
using Fusion;
using Sirenix.OdinInspector;
using Systems.Network;
using UnityEngine;

namespace Managers.Interactions
{
    [RequireComponent(typeof(Collider))]
    [ValidateInput(nameof(ValidateIfHasTag), "An interaction component must be placed on a collider that has the 'Interaction' tag.")]
    [ValidateInput(nameof(ValidateIfColliderIsTrigger), "An interaction component must have a collider of type 'trigger'.")]
    public class Interaction : NetworkBehaviour
    {
        public const string TAG = "Interaction";
        
        public event Action OnInteractedWith; // Only called on host
        public event Action OnInstantFeedback; // Only called on client who's interaction player has authority
        
        [SerializeField] private SpriteMarkerReceptor markerToShowWhenInteractionPossible;

        [Networked] public bool InteractionEnabled { get; set; }

        public int InteractionId => Id.GetHashCode();
        
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
        
        public override void Spawned()
        {
            if (InteractionManager.HasInstance)
            {
                InteractionManager.Instance.RegisterInteraction(this);
            }

            InteractionEnabled = true;
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
            if (!InteractionEnabled)
                return false;
            
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
            
            if (NetworkSystem.Instance.IsPlayer(interacter.Object.InputAuthority))
                OnInstantFeedback?.Invoke();
            
            if (NetworkSystem.Instance.IsHost)
                OnInteractedWith?.Invoke();
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