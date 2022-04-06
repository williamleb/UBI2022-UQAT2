using Canvases.Markers;

namespace Ingredients.Volumes.LocalPlayer
{
    // There must be only one of those in the world. Else, it could create problems with markers not showing up forever.
    public class HideMarkersOutsideCameraVolume : LocalPlayerVolume
    {
        protected override void ActivateImplementation()
        {
            if (MarkerManager.HasInstance) MarkerManager.Instance.HideMarkersOutsideView = true;
        }
        
        protected override void DeactivateImplementation()
        {
            if (MarkerManager.HasInstance) MarkerManager.Instance.HideMarkersOutsideView = false;
        }

        private void OnDestroy()
        {
            if (MarkerManager.HasInstance) MarkerManager.Instance.HideMarkersOutsideView = false;
        }
    }
}