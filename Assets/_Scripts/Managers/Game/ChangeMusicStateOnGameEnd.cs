using System;
using Systems.Sound;
using UnityEngine;

namespace Managers.Game
{
    public class ChangeMusicStateOnGameEnd : MonoBehaviour
    {
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

        private void OnGameStateChanged(GameState gameState)
        {
            if (gameState == GameState.Finished)
            {
                if (SoundSystem.HasInstance) SoundSystem.Instance.SetInMenu();
            }
        }
    }
}