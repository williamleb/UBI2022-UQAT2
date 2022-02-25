using Fusion;
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
        public event Action OnLevelChange;

        public LevelState State { get; private set; }

        public enum LevelState
        {
            LOBBY,
            LEVEL
        }

        public int ActiveSceneIndex { get; private set; }

        public void LoadLobby()
        {
            ActiveSceneIndex = lobbySceneIndex;
            NetworkSystem.Instance.NetworkRunner.SetActiveScene(lobbySceneIndex);
            State = LevelState.LOBBY;
            OnLobbyLoad?.Invoke();
        }
        public void LoadLevel(int nextLevelIndex)
        {
            if (ActiveSceneIndex > 0)
            {
                OnLevelChange?.Invoke();
            }

            ActiveSceneIndex = nextLevelIndex;
            NetworkSystem.Instance.NetworkRunner.SetActiveScene(nextLevelIndex);
            State = LevelState.LEVEL;
            OnLobbyLoad?.Invoke();
        }
    }
}
