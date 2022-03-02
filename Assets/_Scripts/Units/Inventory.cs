using Canvases.Markers;
using Fusion;
using Ingredients.Homework;
using UnityEngine;

namespace Units
{
    public class Inventory : NetworkBehaviour
    {
        private const int NO_HOMEWORK = -1;

        [SerializeField] private SpriteMarkerReceptor marker;

        [Networked(OnChanged = nameof(OnHeldHomeworkChanged))]
        private int HeldHomeworkId { get; set; }

        public bool HasHomework => HeldHomeworkId != NO_HOMEWORK;

        public override void Spawned()
        {
            HeldHomeworkId = NO_HOMEWORK;
        }

        // Should only be called on host
        public void HoldHomework(Homework homework)
        {
            // We drop the homework we currently have so we only have one homework
            DropHomeworkIfHeld();

            HeldHomeworkId = homework.HomeworkId;
        }

        // Should only be called on host
        public void DropEverything()
        {
            DropHomeworkIfHeld();
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

        private void DropHomeworkIfHeld()
        {
            var homework = GetCurrentHomework();
            if (!homework)
                return;
            
            homework.DropInWorld(transform.position);

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