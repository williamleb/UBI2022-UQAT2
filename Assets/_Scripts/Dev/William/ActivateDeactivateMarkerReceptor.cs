using Canvases.Markers;
using Managers.Game;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Dev.William
{
	[RequireComponent(typeof(SpriteMarkerReceptor))]
    public class ActivateDeactivateMarkerReceptor : MonoBehaviour
    {
        private SpriteMarkerReceptor marker;

        private void Awake()
        {
	        marker = GetComponent<SpriteMarkerReceptor>();
        }

        [Button("Activate")]
        private void Activate()
        {
	        marker.Activate();
        }
        
        [Button("Deactivate")]
        private void Deactivate()
        {
	        marker.Deactivate();
        }
    }
}