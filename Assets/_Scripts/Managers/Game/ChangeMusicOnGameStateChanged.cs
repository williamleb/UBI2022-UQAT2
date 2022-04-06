using Fusion;
using Systems;
using Systems.Sound;
using Units.Player;
using UnityEngine;

namespace Managers.Game
{
    public class ChangeMusicOnGameStateChanged : MonoBehaviour
    {
        private PlayerEntity localPlayer;
        
        private GameState GameState => GameManager.HasInstance ? GameManager.Instance.CurrentState : GameState.Running;

        private bool hasPlayedOvertimeSound;
        
        private void Start()
        {
            if (GameManager.HasInstance)
                GameManager.Instance.OnGameStateChanged += OnGameStateChanged;

            UpdateLocalPlayer();
            PlayerEntity.OnPlayerSpawned += OnPlayerSpawned;
        }

        private void OnDestroy()
        {
            if (GameManager.HasInstance)
                GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
            
            PlayerEntity.OnPlayerSpawned -= OnPlayerSpawned;

            if (localPlayer)
                localPlayer.OnInventoryChanged -= UpdateMusicState;
        }

        private void OnPlayerSpawned(NetworkObject _)
        {
            UpdateLocalPlayer();
        }
        
        private void OnGameStateChanged(GameState _)
        {
            UpdateMusicState();
        }

        private void UpdateLocalPlayer()
        {
            var newLocalPlayer = PlayerSystem.Instance.LocalPlayer;

            if (localPlayer && newLocalPlayer != localPlayer)
                localPlayer.OnInventoryChanged -= UpdateMusicState;

            if (newLocalPlayer)
                newLocalPlayer.OnInventoryChanged += UpdateMusicState;                
                
            localPlayer = newLocalPlayer;
        }

        private void UpdateMusicState()
        {
            if (localPlayer && localPlayer.IsHoldingHomework && GameState != GameState.Finished) 
            {
                ActivateHoldingHomeworkState();
            }
            else
            {
                ActivateMusicState();
            }
        }

        private void ActivateMusicState()
        {
            if (GameState == GameState.Overtime)
            {
                if (SoundSystem.HasInstance)
                {
                    SoundSystem.Instance.SetInOvertime();

                    if (!hasPlayedOvertimeSound)
                    {
                        SoundSystem.Instance.PlayOvertimeStartSound();
                        hasPlayedOvertimeSound = true;
                    }
                }
            }
            else if (GameState == GameState.Finished)
            {
                if (SoundSystem.HasInstance) SoundSystem.Instance.SetInScoreboard();
            }
            else
            {
                hasPlayedOvertimeSound = false;
                if (SoundSystem.HasInstance) SoundSystem.Instance.SetInGame();
            }
        }

        private void ActivateHoldingHomeworkState()
        {
            if (SoundSystem.HasInstance) SoundSystem.Instance.SetHoldingHomework();
        }
    }
}