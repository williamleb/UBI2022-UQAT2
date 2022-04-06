using Ingredients.Homework;
using Managers.Interactions;
using Managers.Score;
using Units.Player;
using UnityEngine;
using Utilities.Extensions;
using Utilities.Unity;

namespace Units.AI
{
    [RequireComponent(typeof(Interaction))]
    public class HomeworkHandingStation : MonoBehaviour
    {
        private Interaction giveHomeworkInteraction;

        private Interacter entityThatHasGivenHomeworkThisFrame;
        private bool resetEntityThatHasGivenHomeworkThisFrame;
        
        public Interacter EntityThatHasGivenHomeworkThisFrame
        {
            get
            {
                resetEntityThatHasGivenHomeworkThisFrame = true;
                return entityThatHasGivenHomeworkThisFrame; 
            }
        }
        public bool HasAnEntityGivenHomeworkThisFrame => entityThatHasGivenHomeworkThisFrame != null;

        private void Awake()
        {
            giveHomeworkInteraction = GetComponent<Interaction>();
        }

        private void OnEnable()
        {
            giveHomeworkInteraction.AddValidator(CanGiveHomework);
            giveHomeworkInteraction.OnInstantFeedback += OnGiveHomeworkInstantFeedback;
            giveHomeworkInteraction.OnInteractedWith += OnGiveHomework;
        }

        private void OnDisable()
        {
            giveHomeworkInteraction.RemoveValidator(CanGiveHomework);
            giveHomeworkInteraction.OnInstantFeedback -= OnGiveHomeworkInstantFeedback;
            giveHomeworkInteraction.OnInteractedWith -= OnGiveHomework;
        }

        private bool CanGiveHomework(Interacter interacter)
        {
            var interacterGameObject = interacter.gameObject;
            if (!interacterGameObject.IsAPlayerOrAI())
                return false;

            var inventory = interacterGameObject.GetComponentInEntity<Inventory>();
            if (!inventory)
                return false;

            if (!inventory.HasHomework)
                return false;
            
            return true;
        }

        private void OnGiveHomeworkInstantFeedback(Interacter interacter)
        {
            PlayInstantFeedbackSound(interacter);
        }

        private void OnGiveHomework(Interacter interacter)
        {
            var inventory = interacter.gameObject.GetComponentInEntity<Inventory>();
            Debug.Assert(inventory, $"{nameof(HomeworkHandingStation)} should only be interacted with by actors with an {nameof(Inventory)}");
            Debug.Assert(inventory.HasHomework);
            
            if (interacter.gameObject.IsAPlayer())
                HandHomework(interacter, inventory.HeldHomeworkDefinition);
            
            PlayInteractSound(interacter);

            inventory.RemoveHomework();
            entityThatHasGivenHomeworkThisFrame = interacter;
        }

        private void PlayInstantFeedbackSound(Interacter interacter)
        {
            if (interacter.gameObject.IsAPlayer())
            {
                var player = interacter.gameObject.GetComponentInEntity<PlayerEntity>();
                player.PlayHandInHomeworkSoundLocally();
            }
        }

        private void PlayInteractSound(Interacter interacter)
        {
            if (interacter.gameObject.IsAnAI())
            {
                var ai = interacter.gameObject.GetComponentInEntity<AIEntity>();
                ai.PlayHandInHomeworkSoundOnAllClients();
            }
            else if (interacter.gameObject.IsAPlayer())
            {
                var player = interacter.gameObject.GetComponentInEntity<PlayerEntity>();
                player.PlayHandInHomeworkSoundOnOtherClients();
            }
        }

        private void HandHomework(Interacter interacter, HomeworkDefinition homeworkDefinition)
        {
            var player = interacter.gameObject.GetComponentInEntity<PlayerEntity>();
            player.SetGiving();
            Debug.Assert(player, $"An interacter with the tag {Tags.PLAYER} should have a {nameof(PlayerEntity)}");
            
            if (ScoreManager.HasInstance)
                ScoreManager.Instance.HandHomework(player, homeworkDefinition);
        }
        
        private void LateUpdate()
        {
            if (resetEntityThatHasGivenHomeworkThisFrame)
            {
                entityThatHasGivenHomeworkThisFrame = null;
                resetEntityThatHasGivenHomeworkThisFrame = false;
            }
            else if (entityThatHasGivenHomeworkThisFrame != null)
            {
                resetEntityThatHasGivenHomeworkThisFrame = true;
            }
        }
    }
}