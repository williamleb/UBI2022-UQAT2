using BehaviorDesigner.Runtime.Tasks;
using Managers.Interactions;

namespace Units.AI.Conditionals
{
    [TaskDescription("Returns success when the AI can interact with the teacher to give them their homework")]
    [TaskCategory("AI/Can Interact With")]
    public class CanInteractWithTeacher : CanInteractWith
    {
        protected override bool FilterInteraction(Interaction interaction)
        {
            return interaction.gameObject.GetComponent<HomeworkHandingStation>() != null;
        }
    }
}