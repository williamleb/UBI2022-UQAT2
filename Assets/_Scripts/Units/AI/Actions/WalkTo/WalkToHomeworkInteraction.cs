using BehaviorDesigner.Runtime.Tasks;
using Ingredients.Homework;
using Managers.Interactions;

namespace Units.AI.Actions
{
    [TaskCategory("AI/Walk To")]
    [TaskDescription("Make the AI walk towards a random homework.")]
    public class WalkToHomeworkInteraction : WalkToInteraction
    {
        protected override bool FilterInteraction(Interaction interaction)
        {
            return interaction.gameObject.GetComponent<Homework>() != null;
        }
    }
}