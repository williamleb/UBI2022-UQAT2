using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace Units.AI.Actions
{
    [TaskCategory("AI")]
    public class ResetTransform : Action
    {
        [SerializeField] private readonly SharedTransform transformToReset = null;

        public override TaskStatus OnUpdate()
        {
            transformToReset.SetValue(null);

            return TaskStatus.Success;
        }
    }
}