using BehaviorDesigner.Runtime.Tasks;
using Ingredients.Homework;
using Managers.Interactions;
using UnityEngine;

namespace Units.AI.Actions
{
    [TaskCategory("AIBrain")]
    [TaskDescription("Make the AI walk towards a random homework.")]
    public class WalkTowardsHomeworkInteraction : WalkTowardsInteraction
    {
        protected override bool FilterInteraction(Interaction interaction)
        {
            return interaction.gameObject.GetComponent<Homework>() != null;
        }
    }
}