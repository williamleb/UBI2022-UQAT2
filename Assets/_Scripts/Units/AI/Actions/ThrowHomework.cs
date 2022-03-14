using System;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace Units.AI.Actions
{
    [Serializable]
    [TaskCategory("AI")]
    [TaskDescription("Throws the homework the AI is currently holding.")]
    public class ThrowHomework : AIAction
    {
        [SerializeField] private SharedVector3 direction = Vector3.zero;
        [SerializeField] private SharedFloat force = 0f;

        public override TaskStatus OnUpdate()
        {
            if (!Brain.Inventory || !Brain.Inventory.HasHomework)
                return TaskStatus.Failure;
            
            Brain.Inventory.DropEverything(direction.Value, force.Value);
            return TaskStatus.Success;
        }

        public override void OnReset()
        {
            base.OnReset();
            direction = Vector3.zero;
            force = 0f;
        }
    }
}