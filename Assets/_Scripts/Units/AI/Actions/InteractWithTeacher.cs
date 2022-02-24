using BehaviorDesigner.Runtime.Tasks;
using Managers.Interactions;

namespace Units.AI.Actions
{
    [TaskCategory("AIBrain")]
    [TaskDescription("Interact with the teacher in this AI's interaction list.")]
    public class InteractWithTeacher : InteractWith
    {
        protected override bool FilterInteraction(Interaction interaction)
        {
            return interaction.gameObject.GetComponent<HomeworkHandingStation>() != null;
        }
    }
}