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

        public override void Spawned()
        {
            HeldHomeworkId = NO_HOMEWORK;
        }
        
        // Should only be called on host
        public void HoldHomework(Homework homework)
        {
            // TODO Manage if already holding one
            
            HeldHomeworkId = homework.HomeworkId;
        }
        
        // Should only be called on host
        public void DropEverything()
        {
            // TODO
            Debug.Log($"Drop {gameObject.name}");
        }

        private void UpdateMarkerVisibility()
        {
            if (!marker)
                return;
            
            if (HeldHomeworkId == NO_HOMEWORK)
                marker.Activate();
            else
                marker.Deactivate();
        }
        
        private static void OnHeldHomeworkChanged(Changed<Inventory> changed)
        {
            changed.Behaviour.UpdateMarkerVisibility();
        }
    }
}