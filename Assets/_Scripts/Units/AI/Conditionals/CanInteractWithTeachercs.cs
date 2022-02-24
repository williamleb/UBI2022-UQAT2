using Ingredients.Homework;
using Managers.Interactions;
using Units.AI;

namespace BehaviorDesigner.Runtime.Tasks
{
    [TaskDescription("Returns success when the AI can interact with the teacher to give them their homework")]
    [TaskCategory("AIBrain")]
    public class CanInteractWithTeacher : CanInteractWith
    {
        protected override bool FilterInteraction(Interaction interaction)
        {
            return interaction.gameObject.GetComponent<HomeworkHandingStation>() != null;
        }

    }
}