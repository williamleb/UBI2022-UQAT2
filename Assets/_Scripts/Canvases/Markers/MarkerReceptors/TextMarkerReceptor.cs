using UnityEngine;

namespace Canvases.Markers
{
    public class TextMarkerReceptor : MarkerReceptor<TextMarker>
    {
        [SerializeField] private string text;

        public string Text
        {
            set
            {
                text = value;
                if (CurrentMarker) CurrentMarker.Text = text;
            }
        }
        
        protected override void OnActivated()
        {
            CurrentMarker.Text = text;
        }
    }
}