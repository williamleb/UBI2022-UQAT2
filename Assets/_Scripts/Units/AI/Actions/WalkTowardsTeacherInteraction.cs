using BehaviorDesigner.Runtime.Tasks;
using Managers.Interactions;
using UnityEngine;

namespace Units.AI.Actions
{
    [TaskCategory("AIBrain")]
    [TaskDescription("Make the AI walk towards the teacher.")]
    public class WalkTowardsTeacherInteraction : WalkTowardsInteraction
    {
        protected override bool FilterInteraction(Interaction interaction)
        {
            return interaction.gameObject.GetComponent<HomeworkHandingStation>() != null;
        }
    }
}