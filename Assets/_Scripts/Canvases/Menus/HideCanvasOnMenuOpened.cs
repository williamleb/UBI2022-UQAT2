using UnityEngine;

namespace Canvases.Menu
{
    [RequireComponent(typeof(CanvasGroup))]
    public class HideCanvasOnMenuOpened : MonoBehaviour
    {
        [SerializeField] private bool inverse;
        
        private CanvasGroup canvas;

        private void Awake()
        {
            canvas = GetComponent<CanvasGroup>();
        }

        private void OnEnable()
        {
            if (MenuManager.HasInstance)
                MenuManager.Instance.InMenuStatusChanged += UpdateInMenu;
        }
        
        private void OnDisable()
        {
            if (MenuManager.HasInstance)
                MenuManager.Instance.InMenuStatusChanged -= UpdateInMenu;
        }

        private void UpdateInMenu(bool inMenu)
        {
            canvas.alpha = inMenu == inverse ? 1f : 0f;
        }
    }
}