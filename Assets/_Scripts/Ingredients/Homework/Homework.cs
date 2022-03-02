﻿using System;
using Fusion;
using Managers.Interactions;
using Sirenix.OdinInspector;
using Systems.Sound;
using Units;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Ingredients.Homework
{
    [RequireComponent(typeof(Interaction))]
    [RequireComponent(typeof(Rigidbody))]
    public class Homework : NetworkBehaviour
    {
        private enum State : byte
        {
            InWorld = 0,
            Taken,
            Free
        }

        [SerializeField, Required] private GameObject visual;
        
        [Networked(OnChanged = nameof(OnStateChanged))] private State HomeworkState { get; set; }

        private Interaction interaction;
        private Rigidbody rb;
        private Collider[] colliders;

        public int HomeworkId => Id.GetHashCode();
        public bool IsFree => HomeworkState == State.Free;

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

        public void Free()
        {
            HomeworkState = State.Free;
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
                HomeworkState = State.InWorld;
            }

            HomeworkState = State.Taken;
            inventory.HoldHomework(this);
        }

        public void Spawn(Vector3 position)
        {
            if (HomeworkState != State.Free)
                return;
            
            
        }

        public void DropInWorld(Vector3 position)
        {
            if (HomeworkState != State.Taken)
                return;
            
            HomeworkState = State.InWorld;
            
            transform.position = position + Vector3.up * 2f;
            rb.isKinematic = false;

            // TODO Better launch logic when we will know what the game physics should look like
            var randomAngleX = Random.Range(25f, 60f);
            var randomAngleY = Random.Range(0f, 360f);
            var launchDirection = Quaternion.Euler(randomAngleX, randomAngleY, 0f) * Vector3.up;
            rb.AddForce(launchDirection * 250);
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
            visual.SetActive(HomeworkState == State.InWorld);
            interaction.InteractionEnabled = HomeworkState == State.InWorld;
            foreach (var colliderComponent in colliders)
            {
                colliderComponent.enabled = HomeworkState == State.InWorld;
            }

            rb.isKinematic = HomeworkState != State.InWorld;
        }

        private static void OnStateChanged(Changed<Homework> changed)
        {
            changed.Behaviour.UpdateForCurrentState();
        }
    }
}