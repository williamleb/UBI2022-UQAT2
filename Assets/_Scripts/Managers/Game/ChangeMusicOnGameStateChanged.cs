using Fusion;
using Systems;
using Systems.Sound;
using Units.Player;
using UnityEngine;

namespace Managers.Game
{
    public class ChangeMusicOnGameStateChanged : MonoBehaviour
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
            if (gameState == GameState.Overtime)
            {
                if (SoundSystem.HasInstance)
                {
                    SoundSystem.Instance.SetInOvertime();
                    SoundSystem.Instance.PlayOvertimeStartSound();
                }
            }
            else if (gameState == GameState.Finished)
            {
                if (SoundSystem.HasInstance) SoundSystem.Instance.SetInScoreboard();
            }
            else
            {
                if (SoundSystem.HasInstance) SoundSystem.Instance.SetInGame();
            }
        }
    }
}