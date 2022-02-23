using Sirenix.OdinInspector;
using Units.Player;
using UnityEngine;

namespace Canvases.Markers
{
    public class PromptMarkerReceptor : MarkerReceptor<PromptMarker>
    {
        [SerializeField, ValidateInput(nameof(ValidateAction), "Action is not valid. See class PlayerActionHandler to know valid actions")] 
        private string action = "Interact";
        
        protected override void OnActivated()
        {
            CurrentMarker.Action = action;
        }
        
        private bool ValidateAction()
        {
            return PlayerInputHandler.ValidActions.Contains(action.ToLower());
        }
    }
}