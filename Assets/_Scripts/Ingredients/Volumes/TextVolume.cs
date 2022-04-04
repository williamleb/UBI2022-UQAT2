using Canvases.Markers;
using UnityEngine;

namespace Ingredients.Volumes
{
    [RequireComponent(typeof(LocalPlayerDetection))]
    public class TextVolume : LocalPlayerVolume
    {
        [SerializeField] private TextMarkerReceptor textMarker;

        protected override void ActivateImplementation()
        {
            if (textMarker) textMarker.Activate();
        }

        protected override void DeactivateImplementation()
        {
            if (textMarker) textMarker.Deactivate();
        }
    }
}