using Canvases.Components;
using UnityEngine;

namespace Canvases.Markers
{
    [RequireComponent(typeof(TextUIComponent))]
    public class TextMarker : Marker
    {
        private TextUIComponent text;

        public string Text
        {
            set => text.Text = value;
        }
        
        public Color Color
        {
            set => text.Color = value;
        }
        
        protected override void Awake()
        {
            base.Awake();

            text = GetComponent<TextUIComponent>();
        }
    }
}