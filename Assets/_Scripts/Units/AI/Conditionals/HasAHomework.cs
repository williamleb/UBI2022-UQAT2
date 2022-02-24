using Units.AI;
using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks
{
    [TaskDescription("Returns success when the inventory contains a homework.")]
    [TaskCategory("AIBrain")]
    public class HasAHomework : Conditional
    {
        [Tooltip("The inventory which will be checked to know if we are carrying a homework")]
        [SerializeField] private AIBrain brain = null;

        public override TaskStatus OnUpdate()
        {
            if (!brain)
                return TaskStatus.Failure;
            
            return brain.Inventory.HasHomework ? TaskStatus.Success : TaskStatus.Failure;
        }

        public override void OnReset()
        {
            brain = null;
        }
    }
}