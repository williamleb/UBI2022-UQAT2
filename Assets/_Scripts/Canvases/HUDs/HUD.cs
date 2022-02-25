using Managers.Game;
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

        private void Start()
        {
            if (GameManager.HasInstance)
                GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
        }

        private void OnDestroy()
        {
            if (GameManager.HasInstance)
                GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
        }

        private void OnGameStateChanged(GameState newGameState)
        {
            canvasGroup.alpha = newGameState == GameState.Running ? 1f : 0f;
        }
    }
}