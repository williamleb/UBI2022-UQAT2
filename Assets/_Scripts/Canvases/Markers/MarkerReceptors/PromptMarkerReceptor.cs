using UnityEngine;
using UnityEngine.InputSystem;

namespace Canvases.Markers
{
    public class PromptMarkerReceptor : MarkerReceptor<PromptMarker>
    {
        [SerializeField, Tooltip("Action assigned to the prompt at start")]
        private InputActionReference actionReference;
        
        protected override void OnActivated()
        {
            CurrentMarker.Action = actionReference.action;
        }
    }
}