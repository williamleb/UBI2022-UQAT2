using Ingredients.Homework;
using Managers.Interactions;

namespace BehaviorDesigner.Runtime.Tasks
{
    [TaskDescription("Returns success when the AI can interact with a homework")]
    [TaskCategory("AIBrain")]
    public class CanInteractWithHomework : CanInteractWith
    {
        protected override bool FilterInteraction(Interaction interaction)
        {
            return interaction.gameObject.GetComponent<Homework>() != null;
        }

    }
}