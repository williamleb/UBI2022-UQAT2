using BehaviorDesigner.Runtime.Tasks;
using Ingredients.Homework;
using Managers.Interactions;

namespace Units.AI.Actions
{
    [TaskCategory("AI/Interact With")]
    [TaskDescription("Interact with the first homework in this AI's interaction list.")]
    public class InteractWithHomework : InteractWith
    {
        protected override bool FilterInteraction(Interaction interaction)
        {
            return interaction.gameObject.GetComponent<Homework>() != null;
        }
    }
}