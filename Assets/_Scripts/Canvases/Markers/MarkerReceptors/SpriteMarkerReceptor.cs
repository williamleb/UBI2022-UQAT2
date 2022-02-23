using UnityEngine;

namespace Canvases.Markers
{
    public class SpriteMarkerReceptor : MarkerReceptor<SpriteMarker>
    {
        [SerializeField] private Sprite sprite;
        
        protected override void OnActivated()
        {
            CurrentMarker.Sprite = sprite;
        }
    }
}