using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace Units.AI.Actions
{
    [TaskCategory("Values")]
    public class EvaluateBool : Conditional
    {
        [SerializeField] private readonly SharedBool boolToEvaluate = false;
        [SerializeField] private readonly SharedBool invert = false;

        public override TaskStatus OnUpdate()
        {
            return boolToEvaluate.Value != invert.Value ? TaskStatus.Success : TaskStatus.Failure;
        }
    }
}