using System;
using Canvases.Markers;
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
        private static readonly int isSpawned = Animator.StringToHash("IsSpawned");

        public enum State : byte
        {
            InWorld = 0,
            Taken,
            Free
        }

        public event Action<Homework, State> EventOnStateChanged;

        [SerializeField, Required] private GameObject visual;
        [SerializeField] private SpriteMarkerReceptor homeworkMarker;

        private Interaction interaction;
        private Rigidbody rb;
        private Collider[] colliders;
        private Animator animator;

        private Transform holdingTransform;

        [Networked(OnChanged = nameof(OnStateChanged))]
        public State HomeworkState { get; private set; }
        
        public int HomeworkId => Id.GetHashCode();
        public bool IsFree => HomeworkState == State.Free;

        private void Awake()
        {
            interaction = GetComponent<Interaction>();
            rb = GetComponent<Rigidbody>();
            colliders = GetComponents<Collider>();
            animator = GetComponent<Animator>();
        }

        private void OnEnable()
        {
            interaction.OnInstantFeedback += OnInstantFeedback;
            interaction.OnInteractedWith += OnInteractedWith;
            interaction.OnInteractionPossibilityChanged += OnInteractionPossibilityChanged;
        }

        private void OnDisable()
        {
            interaction.OnInstantFeedback -= OnInstantFeedback;
            interaction.OnInteractedWith -= OnInteractedWith;
            interaction.OnInteractionPossibilityChanged -= OnInteractionPossibilityChanged;
        }

        private void OnInteractionPossibilityChanged(bool interactionPossible)
        {
            UpdateHomeworkMarkerVisibility();            
        }

        private void UpdateHomeworkMarkerVisibility()
        {
            if (!homeworkMarker)
                return;

            if (interaction.Possible || HomeworkState == State.Free)
            {
                homeworkMarker.Deactivate();
            }
            else
            {
                homeworkMarker.Activate();
            }
        }

        private void OnInstantFeedback(Interacter interacter)
        {
            // TODO Instant feedback for picking the item
            SoundSystem.Instance.PlayBababooeySound();
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
            holdingTransform = inventory.HomeworkHoldingTransform;
        }

        public void Free()
        {
            HomeworkState = State.Free;
            holdingTransform = null;
        }

        public void Activate(Transform transformToActivateTo)
        {
            if (HomeworkState != State.Free)
                return;

            HomeworkState = State.InWorld;
            holdingTransform = null;

            var thisTransform = transform;
            thisTransform.position = transformToActivateTo.position;
            thisTransform.rotation = transformToActivateTo.rotation;
            rb.isKinematic = false;
        }

        public void DropInWorld()
        {
            DropInWorld(Vector3.zero);
        }

        public void DropInWorld(Vector3 velocity)
        {
            if (HomeworkState != State.Taken)
                return;
            
            HomeworkState = State.InWorld;
            holdingTransform = null;

            rb.isKinematic = false;
            rb.AddForce(velocity);
        }

        public override void Spawned()
        {
            if (HomeworkManager.HasInstance)
            {
                HomeworkManager.Instance.RegisterHomework(this);
            }

            UpdateHomeworkMarkerVisibility();
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
            visual.SetActive(HomeworkState != State.Free);
            interaction.InteractionEnabled = HomeworkState == State.InWorld;
            foreach (var colliderComponent in colliders)
            {
                colliderComponent.enabled = HomeworkState == State.InWorld;
            }

            rb.isKinematic = HomeworkState != State.InWorld;
            animator.SetBool(isSpawned, HomeworkState != State.Free);
            
            UpdateHomeworkMarkerVisibility();     
            EventOnStateChanged?.Invoke(this, HomeworkState);
        }

        public override void FixedUpdateNetwork()
        {
            if (holdingTransform)
            {
                var thisTransform = transform;
                thisTransform.position = holdingTransform.position;
                thisTransform.rotation = holdingTransform.rotation;
            }
        }

        private static void OnStateChanged(Changed<Homework> changed)
        {
            changed.Behaviour.UpdateForCurrentState();
        }
    }
}