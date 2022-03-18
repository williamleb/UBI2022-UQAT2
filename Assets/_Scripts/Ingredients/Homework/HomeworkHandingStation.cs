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
            // TODO Play instant feedback on give homework
        }

        private void OnGiveHomework(Interacter interacter)
        {
            var inventory = interacter.gameObject.GetComponentInEntity<Inventory>();
            Debug.Assert(inventory, $"{nameof(HomeworkHandingStation)} should only be interacted with by actors with an {nameof(Inventory)}");
            Debug.Assert(inventory.HasHomework);
            
            if (interacter.gameObject.IsAPlayer())
                HandHomework(interacter, inventory.HeldHomeworkDefinition);
            
            inventory.RemoveHomework();
            entityThatHasGivenHomeworkThisFrame = interacter;
        }

        private void HandHomework(Interacter interacter, HomeworkDefinition homeworkDefinition)
        {
            if (!ScoreManager.HasInstance)
                return;
            
            var player = interacter.gameObject.GetComponentInEntity<PlayerEntity>();
            Debug.Assert(player, $"An interacter with the tag {Tags.PLAYER} should have a {nameof(PlayerEntity)}");
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