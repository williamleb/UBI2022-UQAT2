using UnityEngine;

namespace Canvases.HUDs
{
    [RequireComponent(typeof(CanvasGroup))]
    public class HUD : MonoBehaviour
    {
        private CanvasGroup canvasGroup;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }
        
        
    }
}