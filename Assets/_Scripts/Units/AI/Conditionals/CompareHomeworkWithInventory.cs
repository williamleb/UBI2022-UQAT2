using System;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Ingredients.Homework;
using UnityEngine;

namespace Units.AI.Conditionals
{
    [Serializable]
    [TaskDescription("Returns success if the target homework is the same as the one in the AI's inventory.")]
    [TaskCategory("AI")]
    public class CompareHomeworkWithInventory : AIConditional
    {
        [SerializeField] private SharedTransform targetHomework;
        [SerializeField] private SharedBool successOnNoHomework;

        public override TaskStatus OnUpdate()
        {
            if (!targetHomework.Value)
                return successOnNoHomework.Value ? TaskStatus.Success : TaskStatus.Failure; 
            
            var homework = targetHomework.Value.GetComponent<Homework>();
            if (!homework)
                return successOnNoHomework.Value ? TaskStatus.Success : TaskStatus.Failure;

            if (!Brain.Inventory || !Brain.Inventory.HasHomework)
                return TaskStatus.Failure;

            return homework == Brain.Inventory.HeldHomework ? TaskStatus.Success : TaskStatus.Failure;
        }

        public override void OnReset()
        {
            base.OnReset();
            targetHomework = null;
            successOnNoHomework = false;
        }
    }
}