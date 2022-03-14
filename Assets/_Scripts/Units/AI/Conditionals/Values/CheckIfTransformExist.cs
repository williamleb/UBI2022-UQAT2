using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace Units.AI.Actions
{
    [TaskCategory("Values")]
    public class CheckIfTransformExist : Conditional
    {
        [SerializeField] private readonly SharedTransform transformToEvaluate = null;

        public override TaskStatus OnUpdate()
        {
            return transformToEvaluate.Value ? TaskStatus.Success : TaskStatus.Failure;
        }
    }
}