using System;
using Systems.Network;
using Canvases.Markers;
using Fusion;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Managers.Interactions
{
    [RequireComponent(typeof(Collider))]
    [ValidateInput(nameof(ValidateIfHasTag), "An interaction component must be placed on a collider that has the 'Interaction' tag.")]
    public class Interaction : NetworkBehaviour
    {
        public const string TAG = "Interaction";
        
        public event Action OnInteractedWith;

        [SerializeField] private SpriteMarkerReceptor markerToShowWhenInteractionPossible;

        [Networked(OnChanged = nameof(OnHello))] private int Hello {get; set; }

        private void Awake()
        {
            Hello = 0;
        }

        private void OnHello()
        {
            Debug.Log($"Hello {Id}: {Hello}");
        }

        private float timer = 0f;
        private void Update()
        {
            timer += Time.deltaTime;
            if (timer > 1f)
            {
                if (NetworkSystem.Instance.IsHost)
                {
                    Hello = 0;
                    Hello++;
                }
                timer = 0f;
            }
        }

        private bool ValidateIfHasTag()
        {
            return gameObject.CompareTag(TAG);
        }

        private void OnEnable()
        {
            if (InteractionManager.HasInstance)
                InteractionManager.Instance.RegisterInteraction(this);
        }

        private void OnDisable()
        {
            if (InteractionManager.HasInstance)
                InteractionManager.Instance.UnregisterInteraction(this);
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
    }
}