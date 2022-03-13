using System.Linq;
using Canvases.Markers;
using Fusion;
using Ingredients.Homework;
using Interfaces;
using Systems.Settings;
using UnityEngine;

namespace Units
{
    public class Inventory : NetworkBehaviour
    {
        private const int NO_HOMEWORK = -1;

        [SerializeField] private SpriteMarkerReceptor marker;
        [SerializeField] private Transform homeworkHoldingTransform;

        private IVelocityObject velocityObject;

        [Networked(OnChanged = nameof(OnHeldHomeworkChanged))]
        private int HeldHomeworkId { get; set; }

        public bool HasHomework => HeldHomeworkId != NO_HOMEWORK;
        public HomeworkDefinition HeldHomeworkDefinition => HasHomework ? SettingsSystem.HomeworkSettings.HomeworkDefinitions.FirstOrDefault(definition => definition.Type.Equals(GetCurrentHomework().Type)) : null;
        public Transform HomeworkHoldingTransform => homeworkHoldingTransform;

        private Vector3 VelocityContribution => velocityObject != null ? velocityObject.Velocity * SettingsSystem.HomeworkSettings.CurrentObjectContributionToHomeworkFalling : Vector3.zero;

        private void Awake()
        {
            if (!homeworkHoldingTransform)
                homeworkHoldingTransform = transform;
        }

        public void AssignVelocityObject(IVelocityObject objectToTakeVelocityFrom)
        {
            velocityObject = objectToTakeVelocityFrom;
        }

        public override void Spawned()
        {
            HeldHomeworkId = NO_HOMEWORK;
        }

        // Should only be called on host
        public void HoldHomework(Homework homework)
        {
            // We drop the homework we currently have so we only have one homework
            DropHomeworkIfHeld(Vector3.zero, 0f);

            HeldHomeworkId = homework.HomeworkId;
        }

        // Should only be called on host
        public void DropEverything()
        {
            DropEverything(Vector3.zero, 0f);
        }
        
        // Should only be called on host
        /// <param name="direction">The direction of the force that causes the drop.
        ///                         The dropped items might not fall exactly in this direction since the inventory's velocity also affects this direction.
        ///                         This direction will be normalized. You should use the force attribute to change the force at which everything is dropped.</param>
        /// <param name="force">The force at which the object should be dropped in the specified direction.
        ///                     A force of 1 represent the same impact as the object's current velocity.</param>
        public void DropEverything(Vector3 direction, float force)
        {
            DropHomeworkIfHeld(direction, force);
        }

        // Should only be called on host
        public void RemoveHomework()
        {
            if (!HasHomework)
                return;
            
            if (!HomeworkManager.HasInstance)
                return;
            
            HomeworkManager.Instance.RemoveHomework(HeldHomeworkId);

            HeldHomeworkId = NO_HOMEWORK;
        }

        private void DropHomeworkIfHeld(Vector3 impactDirection, float impactForce)
        {
            var homework = GetCurrentHomework();
            if (!homework)
                return;

            var impactContribution = impactDirection.normalized * impactForce * SettingsSystem.HomeworkSettings.ImpactContributionToHomeworkFalling;
            var launchVelocity = impactContribution + VelocityContribution;
            
            homework.DropInWorld(launchVelocity);

            HeldHomeworkId = NO_HOMEWORK;
        }

        private Homework GetCurrentHomework()
        {
            if (!HasHomework)
                return null;

            if (!HomeworkManager.HasInstance)
                return null;
            
            return HomeworkManager.Instance.GetHomework(HeldHomeworkId);
        }

        private void UpdateMarkerVisibility()
        {
            if (!marker)
                return;
            
            if (HeldHomeworkId == NO_HOMEWORK)
                marker.Deactivate();
            else
                marker.Activate();
        }
        
        private static void OnHeldHomeworkChanged(Changed<Inventory> changed)
        {
            changed.Behaviour.UpdateMarkerVisibility();
        }
    }
}