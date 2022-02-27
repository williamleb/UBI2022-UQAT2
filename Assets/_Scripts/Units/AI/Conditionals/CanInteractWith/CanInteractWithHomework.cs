using BehaviorDesigner.Runtime.Tasks;
using Ingredients.Homework;
using Managers.Interactions;

namespace Units.AI.Conditionals
{
    [TaskDescription("Returns success when the AI can interact with a homework")]
    [TaskCategory("AI/Can Interact With")]
    public class CanInteractWithHomework : CanInteractWith
    {
        protected override bool FilterInteraction(Interaction interaction)
        {
            return interaction.gameObject.GetComponent<Homework>() != null;
        }
    }
}