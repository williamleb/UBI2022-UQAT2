using Fusion;
using System;
using Systems.Network;
using Trisibo;
using UnityEngine;
using Utilities.Singleton;

namespace Systems
{
    public class LevelSystem : PersistentSingleton<LevelSystem>
    {
        [SerializeField] private SceneField mainMenuSceneIndex;
        [SerializeField] private SceneField lobbySceneIndex;
        [SerializeField] private SceneField gameSceneIndex;

        public event Action OnLobbyLoad;
        public event Action OnGameLoad;

        public LevelState State { get; private set; }

        public enum LevelState
        {
            TRANSITION,
            LOBBY,
            GAME
        }

        public void Start()
        {
            NetworkSystem.Instance.OnSceneLoadDoneEvent += ChangeLevelState;
        }

        public int ActiveSceneIndex { get; private set; }

        public void LoadLobby()
        {   
            State = LevelState.TRANSITION;

            Debug.Log("Loading lobby scene.");
            ActiveSceneIndex = lobbySceneIndex.BuildIndex;
            NetworkSystem.Instance.NetworkRunner.SetActiveScene(lobbySceneIndex.BuildIndex);
        }

        private void ChangeLevelState(NetworkRunner networkRunner)
        {   
            if (ActiveSceneIndex == lobbySceneIndex.BuildIndex)
            {
                State = LevelState.LOBBY;
                OnLobbyLoad?.Invoke();
            }
            else if (ActiveSceneIndex == gameSceneIndex.BuildIndex)
            {
                State = LevelState.GAME;
                OnGameLoad?.Invoke();
            }
            else
            {
                State = LevelState.TRANSITION;
            }

            Debug.Log($"Scene loaded with build index {ActiveSceneIndex}.");
        }

        public void LoadGame()
        {
            State = LevelState.TRANSITION;

            if (PlayerSystem.Instance.AllPlayers.Count != 0)
                PlayerSystem.Instance.DespawnAllPlayers();

            Debug.Log($"Loading scene with index {gameSceneIndex.BuildIndex}");
            ActiveSceneIndex = gameSceneIndex.BuildIndex;
            NetworkSystem.Instance.NetworkRunner.SetActiveScene(gameSceneIndex.BuildIndex);
        }
    }
}
