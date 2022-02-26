using Fusion;
using Managers.Game;
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
        [SerializeField] private NetworkObject gameManagerPrefab;

        public event Action OnLobbyLoad;
        public event Action OnLevelLoad;

        public LevelState State { get; private set; }

        public enum LevelState
        {
            OTHER,
            LOBBY,
            LEVEL
        }

        public int ActiveSceneIndex { get; private set; }

        public void LoadLobby()
        {
            Debug.Log("Loading lobby scene.");
            ActiveSceneIndex = lobbySceneIndex;
            NetworkSystem.Instance.NetworkRunner.SetActiveScene(lobbySceneIndex);
            State = LevelState.LOBBY;
            SpawnManager();
            OnLobbyLoad?.Invoke();
        }

        //Quick fix to spawn GameManager before player. To see if we can decouple playerentity to this manager.
        private void SpawnManager()
        {
            Debug.Log("Spawning GameManager.");
            NetworkSystem.Instance.NetworkRunner.Spawn(gameManagerPrefab,null, null);
        }

        public void LoadLevel(int nextLevelIndex)
        {
            Debug.Log($"Loading scene with index {nextLevelIndex}");
            if (ActiveSceneIndex < 0)
            {
                LoadLobby();
            }

            ActiveSceneIndex = nextLevelIndex;
            NetworkSystem.Instance.NetworkRunner.SetActiveScene(nextLevelIndex);
            State = LevelState.LEVEL;
            SpawnManager();
            OnLevelLoad?.Invoke();
        }
    }
}
