using Canvases.Markers;
using Managers.Game;
using UnityEngine;

namespace Dev.William
{
	[RequireComponent(typeof(SpriteMarkerReceptor))]
    public class ActivateMarkerReceptorAtStart : MonoBehaviour
    {
        private SpriteMarkerReceptor marker;

        private void Awake()
        {
	        marker = GetComponent<SpriteMarkerReceptor>();
        }

        private void Start()
		{
			if (GameManager.HasInstance)
			{
				GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
			}
		}

        private void OnGameStateChanged(GameState state)
        {
	        if (state == GameState.Running)
	        {
		        marker.Activate();
	        }
        }
    }
}