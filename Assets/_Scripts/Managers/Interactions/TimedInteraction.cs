using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Managers.Interactions
{
    public class TimedInteraction : Interaction
    {
        // TODO progress marker when progress > 0
        
        [SerializeField] private bool timedInteraction = false;
        [SerializeField, HideIf(nameof(timedInteraction))] private float secondsOfInteraction = 2f;

        private float currentInteractionTime = 0f;
        private bool wasInteractedWithThisFrame = false;
        
        public float InteractionProgress => secondsOfInteraction <= 0f ? 1f : currentInteractionTime / secondsOfInteraction;
        
        private void LateUpdate()
        {
            if (!wasInteractedWithThisFrame)
                currentInteractionTime = Math.Max(0f, currentInteractionTime - Time.deltaTime);
            
            wasInteractedWithThisFrame = false;
        }
        
        public override void Interact()
        {
            wasInteractedWithThisFrame = true;

            currentInteractionTime += Time.deltaTime;
            if (currentInteractionTime > secondsOfInteraction)
            {
                currentInteractionTime = 0f;
                OnInteraction();
            }
        }
    }
}