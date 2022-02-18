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

        [Networked(OnChanged = nameof(OnHeldHomeworkChanged))] private int HeldHomeworkId { get; set; }

        private bool HasHomework => HeldHomeworkId != NO_HOMEWORK;

        public override void Spawned()
        {
            HeldHomeworkId = NO_HOMEWORK;
        }
        
        // Should only be called on host
        public void HoldHomework(Homework homework)
        {
            DropHomework();
            
            HeldHomeworkId = homework.HomeworkId;
        }
        
        // Should only be called on host
        public void DropEverything()
        {
            DropHomework();
        }

        private void DropHomework()
        {
            if (!HasHomework)
                return;
            
            if (HomeworkManager.HasInstance)
            {
                var homework = HomeworkManager.Instance.GetHomework(HeldHomeworkId);
                if (homework)
                    homework.Free(transform.position);
            }
            HeldHomeworkId = NO_HOMEWORK;
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