using System.Collections.Generic;
using Managers.Game;
using UnityEngine;

namespace Canvases.Utilities
{
    [RequireComponent(typeof(CanvasGroup))]
    public class OnlyDisplayForAGameState : MonoBehaviour
    {
        [SerializeField] private List<GameState> gameStatesToDisplayFor = new List<GameState>();

        private CanvasGroup canvasGroup;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        private void Start()
        {
            if (!GameManager.HasInstance)
                return;
            
            GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
            UpdateVisibility(GameManager.Instance.CurrentState);
        }

        private void OnDestroy()
        {
            if (!GameManager.HasInstance)
                return;
            
            GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
        }

        private void OnGameStateChanged(GameState newGameState)
        {
            UpdateVisibility(newGameState);
        }

        private void UpdateVisibility(GameState state)
        {
            canvasGroup.alpha = gameStatesToDisplayFor.Contains(state) ? 1f : 0f;
        }
    }
}