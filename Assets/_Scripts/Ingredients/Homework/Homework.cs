using System;
using Canvases.Markers;
using Fusion;
using Ingredients.Volumes.WorldObjects;
using Managers.Interactions;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Units;
using Units.AI;
using Units.Player;
using UnityEngine;
using Utilities.Extensions;

namespace Ingredients.Homework
{
    [RequireComponent(typeof(Interaction))]
    [RequireComponent(typeof(Rigidbody))]
    public class Homework : NetworkBehaviour, IWorldObject
    {
        private static readonly int IsSpawned = Animator.StringToHash("IsSpawned");

        private enum State : byte
        {
            InWorld = 0,
            Taken,
            Free
        }
        
        public static event Action<Homework> OnHomeworkSpawned;
        public static event Action<Homework> OnHomeworkDespawned;

        public event Action<Homework> EventOnStateChanged;

        [SerializeField, Required] private GameObject visual;
        [SerializeField] private SpriteMarkerReceptor homeworkMarker;

        private Interaction interaction;
        private Rigidbody rb;
        private Collider[] colliders;
        private Animator animator;

        private Transform holdingTransform;

        [Networked, Capacity(8)]
        public string Type { get; private set; }
        
        [Networked(OnChanged = nameof(OnStateChanged))]
        private State HomeworkState { get; set; }
        
        public int HomeworkId => Id.GetHashCode();
        public bool IsFree => HomeworkState == State.Free;
        public bool IsInWorld => HomeworkState == State.InWorld;
        public bool IsTaken => HomeworkState == State.Taken;
        public Vector3 Position => transform.position;

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

            if (interaction.Possible || IsFree)
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
            PlayInstantFeedbackSound(interacter);
            interaction.InteractionEnabled = false;
        }

        private void OnInteractedWith(Interacter interacter)
        {
            if (IsTaken) return;
            
            PlayInteractSound(interacter);
            PlayGrabAnim(interacter);
            
            if (!interacter.TryGetComponent(out Inventory inventory))
            {
                Debug.LogWarning("Homework collected by an interacter without an inventory. Reverting to free state.");
                HomeworkState = State.InWorld;
                return;
            }

            HomeworkState = State.Taken;
            inventory.HoldHomework(this);
            holdingTransform = inventory.HomeworkHoldingTransform;
        }

        private void PlayInstantFeedbackSound(Interacter interacter)
        {
            if (interacter.gameObject.IsAPlayer())
            {
                var player = interacter.gameObject.GetComponentInEntity<PlayerEntity>();
                player.PlayPickUpHomeworkSoundLocally();
            }
        }
        
        private void PlayGrabAnim(Interacter interacter)
        {
            if (interacter.gameObject.IsAPlayer())
            {
                var player = interacter.gameObject.GetComponentInEntity<PlayerEntity>();
                player.SetGrabbing();
            }
        }

        private void PlayInteractSound(Interacter interacter)
        {
            if (interacter.gameObject.IsAnAI())
            {
                var ai = interacter.gameObject.GetComponentInEntity<AIEntity>();
                ai.PlayPickUpHomeworkSoundOnAllClients();
            }
            else if (interacter.gameObject.IsAPlayer())
            {
                var player = interacter.gameObject.GetComponentInEntity<PlayerEntity>();
                player.PlayPickUpHomeworkSoundOnOtherClients();
            }
        }

        public void AssignType(string type)
        {
            Debug.Assert(!type.IsNullOrWhitespace());
            Type = type;
        }

        public void Free()
        {
            HomeworkState = State.Free;
            holdingTransform = null;
        }

        public void Activate(Transform transformToActivateTo)
        {
            if (!IsFree) return;

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
            if (!IsTaken) return;
            
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

            if (Type.IsNullOrWhitespace())
            {
                Type = "Default";
            }

            UpdateHomeworkMarkerVisibility();
            
            OnHomeworkSpawned?.Invoke(this);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (HomeworkManager.HasInstance)
            {
                HomeworkManager.Instance.UnregisterHomework(this);
            }
            
            OnHomeworkDespawned?.Invoke(this);
        }

        private void UpdateForCurrentState()
        {
            visual.SetActive(!IsFree);
            interaction.InteractionEnabled = IsInWorld;
            foreach (var colliderComponent in colliders)
            {
                colliderComponent.enabled = IsInWorld;
            }

            rb.isKinematic = !IsInWorld;
            animator.SetBool(IsSpawned, !IsFree);
            
            UpdateHomeworkMarkerVisibility();     
            EventOnStateChanged?.Invoke(this);
        }

        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();
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
        
        public void OnEscapedWorld()
        {
            // TODO
        }
    }
}