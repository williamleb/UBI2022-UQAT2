using System;
using System.Collections.Generic;
using Canvases.Markers;
using Fusion;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities.Extensions;

namespace Managers.Interactions
{
    [RequireComponent(typeof(Collider))]
    [ValidateInput(nameof(ValidateIfHasTag), "An interaction component must be placed on a collider that has the 'Interaction' tag.")]
    public class Interaction : NetworkBehaviour
    {
        public const string TAG = "Interaction";
        
        public event Action<Interacter> OnInteractedWith; // Only called on host
        public event Action<Interacter> OnInstantFeedback; // Only called on client who's interaction player has authority
        
        [SerializeField] private SpriteMarkerReceptor markerToShowWhenInteractionPossible;

        private List<Func<Interacter, bool>> validators = new List<Func<Interacter, bool>>();
        
        private bool interactionEnabled = true;
        private bool interactionPossible;

        [Networked(OnChanged = nameof(OnEnabledChanged)), UnityNonSerialized] public bool InteractionEnabled { get; set; }
        
        public int InteractionId => Id.GetHashCode();
        
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

        public void AddValidator(Func<Interacter, bool> validator)
        {
            validators.Add(validator);
        }
        
        public void RemoveValidator(Func<Interacter, bool> validator)
        {
            validators.Remove(validator);
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
            if (!interactionEnabled)
                return false;

            foreach (var validator in validators)
            {
                if (!validator.Invoke(interacter))
                    return false;
            }
            
            if (!Physics.Raycast(transform.position, interacter.transform.position - transform.position, out hit))
                return false;

            if (hit.collider.gameObject.GetParent() != interacter.gameObject && hit.collider.gameObject != interacter.gameObject)
                return false;
            
            return true;
        }

        public void Interact(Interacter interacter)
        {
            if (!CanInteract(interacter))
                return;
            
            if (interacter.Object.HasInputAuthority)
                OnInstantFeedback?.Invoke(interacter);
            
            if (interacter.Object.HasStateAuthority)
                OnInteractedWith?.Invoke(interacter);
        }
        
        private bool ValidateIfHasTag()
        {
            return gameObject.CompareTag(TAG);
        }

        private void UpdateInteractionEnabled()
        {
            // Since we need to know its value outside of network updates
            interactionEnabled = InteractionEnabled;
        }
        
        private static void OnEnabledChanged(Changed<Interaction> changed)
        {
            changed.Behaviour.UpdateInteractionEnabled();
        }
    }
}