using BehaviorDesigner.Runtime.Tasks;
using Managers.Interactions;

namespace Units.AI.Actions
{
    [TaskCategory("AI/Walk To")]
    [TaskDescription("Make the AI walk towards the teacher.")]
    public class WalkToTeacherInteraction : WalkToInteraction
    {
        protected override bool FilterInteraction(Interaction interaction) 
        {
            return interaction.gameObject.GetComponent<HomeworkHandingStation>() != null;
        }
    }
}