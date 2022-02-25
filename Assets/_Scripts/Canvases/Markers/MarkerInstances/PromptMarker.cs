using Canvases.Prompts;
using UnityEngine;

namespace Canvases.Markers
{
    [RequireComponent(typeof(Prompt))]
    public class PromptMarker : Marker
    {
        private Prompt prompt;

        public string Action
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