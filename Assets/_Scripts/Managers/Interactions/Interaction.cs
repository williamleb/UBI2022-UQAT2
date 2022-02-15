using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Managers.Interactions
{
    [RequireComponent(typeof(Collider))]
    [ValidateInput(nameof(ValidateIfHasTag), "An interaction component must be placed on a collider that has the 'Interaction' tag.")]
    public class Interaction : MonoBehaviour
    {
        public const string TAG = "Interaction";
        
        public event Action OnInteractedWith;

        [Header("Interaction Type")]
        [SerializeField] private bool timedInteraction = false;
        [SerializeField, HideIf(nameof(timedInteraction))] private float secondsOfInteraction = 2f;

        private float currentInteractionTime = 0f;
        private bool wasInteractedWithThisFrame = false;

        public float InteractionProgress => secondsOfInteraction <= 0f ? 1f : currentInteractionTime / secondsOfInteraction;

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

        private void OnInteraction()
        {
            OnInteractedWith?.Invoke();
        }

        private void LateUpdate()
        {
            if (!wasInteractedWithThisFrame)
                currentInteractionTime = Math.Max(0f, currentInteractionTime - Time.deltaTime);
            
            wasInteractedWithThisFrame = false;
        }

        public bool CanInteract(Transform transform)
        {
            // TODO Raycast
            return true;
        }

        public void Interact(float deltaTime = 0f)
        {
            wasInteractedWithThisFrame = true;

            if (!timedInteraction)
            {
                OnInteraction();
                return;
            }

            currentInteractionTime += deltaTime;
            if (currentInteractionTime > secondsOfInteraction)
            {
                currentInteractionTime = 0f;
                OnInteraction();
            }
        }

        public int CompareTo(object obj)
        {
            return 0;
        }

        public int Compare(Interaction x, Interaction y)
        {
            return 0;
        }
    }
}