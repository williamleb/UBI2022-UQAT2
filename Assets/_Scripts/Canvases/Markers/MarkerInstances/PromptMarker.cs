using Canvases.Prompts;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Canvases.Markers
{
    [RequireComponent(typeof(Prompt))]
    public class PromptMarker : Marker
    {
        private Prompt prompt;

        public InputAction Action
        {
            set => prompt.Action = value;
        }
        
        protected override void Awake()
        {
            base.Awake();

            prompt = GetComponent<Prompt>();
        }
    }
}