using System;
using UnityEngine;

namespace Canvases.Markers
{
    public class TextMarkerReceptor : MarkerReceptor<TextMarker>
    {
        [SerializeField, TextArea] private string text;
        [SerializeField] private Color color = Color.white;

        public string Text
        {
            set
            {
                text = value;
                if (CurrentMarker) CurrentMarker.Text = text;
            }
        }
        
        public Color Color
        {
            set
            {
                color = value;
                if (CurrentMarker) CurrentMarker.Color = color;
            }
        }
        
        protected override void OnActivated()
        {
            CurrentMarker.Text = text;
            CurrentMarker.Color = color;
        }

        private void OnValidate()
        {
            text = text.Replace("\\n", "\n");
        }
    }
}