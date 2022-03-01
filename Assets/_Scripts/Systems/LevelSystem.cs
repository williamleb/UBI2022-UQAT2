using System;
using Systems.Network;
using UnityEngine;
using Utilities.Singleton;

namespace Systems
{
    public class LevelSystem : PersistentSingleton<LevelSystem>
    {
        [SerializeField] private int mainMenuSceneIndex = 0;
        [SerializeField] private int lobbySceneIndex = 1;
        [SerializeField] private int gameSceneIndex = 2;

        public event Action OnLobbyLoad;
        public event Action OnGameLoad;

        public LevelState State { get; private set; }

        public enum LevelState
        {
            TRANSITION,
            LOBBY,
            GAME
        }

        public int ActiveSceneIndex { get; private set; }

        public void LoadLobby()
        {
            State = LevelState.TRANSITION;
            Debug.Log("Loading lobby scene.");
            ActiveSceneIndex = lobbySceneIndex;
            NetworkSystem.Instance.NetworkRunner.SetActiveScene(lobbySceneIndex);
            State = LevelState.LOBBY;
            OnLobbyLoad?.Invoke();
        }

        public void LoadGame()
        {
            State = LevelState.TRANSITION;

            if (PlayerSystem.Instance.AllPlayers.Count != 0)
                PlayerSystem.Instance.DespawnAllPlayers();

            Debug.Log($"Loading scene with index {gameSceneIndex}");
            ActiveSceneIndex = gameSceneIndex;
            NetworkSystem.Instance.NetworkRunner.SetActiveScene(gameSceneIndex);
            State = LevelState.GAME;
            OnGameLoad?.Invoke();
        }
    }
}
