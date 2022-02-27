﻿using System;
using Fusion;
using UnityEngine;

namespace Managers.Game
{
    public class NetworkedGameData : NetworkBehaviour
    {
        public event Action OnPhaseTotalHomeworkChanged;
        public event Action<GameState> OnGameStateChanged;

        [Networked(OnChanged = nameof(HandlePhaseTotalHomeworkChanged))] public int PhaseTotalHomework { get; set; }
        [Networked(OnChanged = nameof(HandleGameStateChanged))] public bool GameIsStarted { get; set; }
        [Networked(OnChanged = nameof(HandleGameStateChanged))] public bool GameIsEnded { get; set; }

        public override void Spawned()
        {
            PhaseTotalHomework = 0;
        }

        public void Reset()
        {
            PhaseTotalHomework = 0;
            GameIsEnded = false;
            GameIsStarted = false;
        }

        private static void HandlePhaseTotalHomeworkChanged(Changed<NetworkedGameData> changed)
        {
            changed.Behaviour.OnPhaseTotalHomeworkChanged?.Invoke();
        }
        
        private static void HandleGameStateChanged(Changed<NetworkedGameData> changed)
        {
            var networkedData = changed.Behaviour;

            if (networkedData.GameIsEnded)
            {
                Debug.Assert(networkedData.GameIsStarted, "Reached broken game state where game is ended but has not been started.");
                networkedData.OnGameStateChanged?.Invoke(GameState.Finished);
            }
            else
            {
                networkedData.OnGameStateChanged?.Invoke(networkedData.GameIsStarted ? GameState.Running : GameState.NotStarted);
            }
        }

    }
}