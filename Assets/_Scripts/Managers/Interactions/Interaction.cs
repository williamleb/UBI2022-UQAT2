using System;
using System.Collections.Generic;
using Canvases.Markers;
using Fusion;
using UnityEngine;
using Utilities.Extensions;
using Utilities.Unity;

namespace Managers.Interactions
{
    public class Interaction : NetworkBehaviour
    {
        public event Action<Interacter> OnInteractedWith; // Only called on host
        public event Action<Interacter> OnInstantFeedback; // Only called on client who's interaction player has authority
        public event Action<bool> OnInteractionPossibilityChanged;
        
        [SerializeField] private PromptMarkerReceptor markerToShowWhenInteractionPossible;

        private readonly List<Func<Interacter, bool>> validators = new List<Func<Interacter, bool>>();
        
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
                
                OnInteractionPossibilityChanged?.Invoke(interactionPossible);
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

            if (!interacter.gameObject.CompareEntities(hit.collider.gameObject))
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

        private void UpdateInteractionEnabled()
        {
            // Since we need to know its value outside of network updates
            interactionEnabled = InteractionEnabled;
        }
        
        private static void OnEnabledChanged(Changed<Interaction> changed)
        {
            changed.Behaviour.UpdateInteractionEnabled();
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!Application.isPlaying)
                UnityEditor.EditorApplication.delayCall += AssignTagAndLayer;
        }

        private void AssignTagAndLayer()
        {
            if (this == null)
                return;
            
            var thisGameObject = gameObject;
            
            if (!thisGameObject.AssignTagIfDoesNotHaveIt(Tags.INTERACTION))
                Debug.LogWarning($"Player {thisGameObject.name} should have the tag {Tags.INTERACTION}. Instead, it has {thisGameObject.tag}");
            
            if (!thisGameObject.AssignLayerIfDoesNotHaveIt(Layers.GAMEPLAY))
                Debug.LogWarning($"Player {thisGameObject.name} should have the layer {Layers.GAMEPLAY} ({Layers.NAME_GAMEPLAY}). Instead, it has {thisGameObject.layer}");
        }
#endif
    }
}