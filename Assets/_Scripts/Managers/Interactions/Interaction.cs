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

        public bool CanInteract(Transform transform)
        {
            // TODO Raycast
            return true;
        }

        protected virtual void Interact()
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