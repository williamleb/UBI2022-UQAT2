using System;
using Fusion;
using Managers.Interactions;
using Sirenix.OdinInspector;
using Systems.Sound;
using Units;
using UnityEngine;

namespace Ingredients.Homework
{
    [RequireComponent(typeof(Interaction))]
    [RequireComponent(typeof(Rigidbody))]
    public class Homework : NetworkBehaviour
    {
        private enum State : byte
        {
            Free,
            Taken
        }

        [SerializeField, Required] private GameObject visual;
        
        [Networked] private State HomeworkState { get; set; }

        private Interaction interaction;
        private Rigidbody rb;
        private Collider[] colliders;

        public int HomeworkId => Id.GetHashCode();

        private void Awake()
        {
            interaction = GetComponent<Interaction>();
            rb = GetComponent<Rigidbody>();
            colliders = GetComponents<Collider>();
        }

        private void OnEnable()
        {
            interaction.OnInstantFeedback += OnInstantFeedback;
            interaction.OnInteractedWith += OnInteractedWith;
        }

        private void OnInstantFeedback(Interacter interacter)
        {
            // TODO Instant feedback for picking the item
            SoundSystem.Instance.PlayBababooeySound();
            visual.SetActive(false);
            interaction.InteractionEnabled = false;
        }

        private void OnInteractedWith(Interacter interacter)
        {
            if (HomeworkState == State.Taken)
                return;
            
            var inventory = interacter.GetComponent<Inventory>();
            if (!inventory)
            {
                Debug.LogWarning("Homework collected by an interacter without an inventory. Reverting to free state.");
                HomeworkState = State.Free;
            }

            HomeworkState = State.Taken;
            inventory.HoldHomework(this);
        }

        public void Free(Vector3 position)
        {
            if (HomeworkState == State.Free)
                return;
            
            HomeworkState = State.Free;
            
            transform.position = position + Vector3.up * 2f;
            rb.AddForce(Vector3.up * 5f);
        }

        public override void Spawned()
        {
            if (HomeworkManager.HasInstance)
            {
                HomeworkManager.Instance.RegisterHomework(this);
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (HomeworkManager.HasInstance)
            {
                HomeworkManager.Instance.UnregisterHomework(this);
            }
        }

        private void UpdateForCurrentState()
        {
            visual.SetActive(HomeworkState == State.Free);
            interaction.InteractionEnabled = HomeworkState == State.Free;
            foreach (var colliderComponent in colliders)
            {
                colliderComponent.enabled = HomeworkState == State.Free;
            }
        }

        private static void OnStateChanged(Changed<Homework> changed)
        {
            changed.Behaviour.UpdateForCurrentState();
        }
    }
}