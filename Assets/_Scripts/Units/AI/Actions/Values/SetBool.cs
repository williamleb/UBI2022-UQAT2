using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace Units.AI.Actions
{
    [TaskCategory("Values")]
    public class SetBool : Action
    {
        [SerializeField] private readonly SharedBool boolToSet = false;
        [SerializeField] private readonly SharedBool newValue = false;

        public override TaskStatus OnUpdate()
        {
            boolToSet.SetValue(newValue.Value);

            return TaskStatus.Success;
        }
    }
}