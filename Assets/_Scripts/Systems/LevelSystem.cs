using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using Systems.Network;
using Trisibo;
using Units.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utilities.Singleton;

namespace Systems
{
    public class LevelSystem : PersistentSingleton<LevelSystem>
    {
        [SerializeField] private SceneField mainMenuSceneIndex;
        [SerializeField] private SceneField lobbySceneIndex;
        [SerializeField] private SceneField gameSceneIndex;

        protected Scene loadedScene;

        public event Action OnLobbyLoad;
        public event Action OnGameLoad;
        public INetworkSceneObjectProvider networkSceneObjectProvider { get; private set; }

        public LevelState State { get; private set; }

        public enum LevelState
        {
            TRANSITION,
            LOBBY,
            GAME
        }

        public void Start()
        {
            networkSceneObjectProvider = gameObject.AddComponent<NetworkSceneMaganer>();
        }

        // Since the NetworkRunner is deleted after a connection error (idk why),
        // called by the runner to re-register actions
        public void SubscribeNetworkEvents()
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

        public void LoadGame()
        {
            State = LevelState.TRANSITION;

            Debug.Log($"Loading scene with index {gameSceneIndex.BuildIndex}");
            ActiveSceneIndex = gameSceneIndex.BuildIndex;
            NetworkSystem.Instance.NetworkRunner.SetActiveScene(gameSceneIndex.BuildIndex);
        }
        private void ChangeLevelState(NetworkRunner networkRunner)
        {
            if (ActiveSceneIndex == lobbySceneIndex.BuildIndex)
            {
                Debug.Log("Invoking spawn player");
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

            PlayerInputHandler.FetchInput = true;
        }

        public class NetworkSceneMaganer : NetworkSceneManagerBase
        {
            protected override IEnumerator SwitchScene(SceneRef prevScene, SceneRef newScene, FinishedLoadingDelegate finished)
            {
                if (newScene <= 0)
                {
                    finished(new List<NetworkObject>());
                    yield break;
                }

                if (prevScene > 0)
                {
                    PlayerInputHandler.FetchInput = false;
                }

                if (Instance.loadedScene != default)
                {
                    yield return SceneManager.UnloadSceneAsync(Instance.loadedScene);
                }

                Instance.loadedScene = default;

                List<NetworkObject> sceneObjects = new List<NetworkObject>();

                if (newScene >= 0)
                {
                    yield return SceneManager.LoadSceneAsync(newScene, LoadSceneMode.Additive);
                    Instance.loadedScene = SceneManager.GetSceneByBuildIndex(newScene);
                    sceneObjects = FindNetworkObjects(Instance.loadedScene, disable: false);
                }

                // Delay one frame
                yield return null;

                Debug.Log($"Switched Scene from {prevScene} to {newScene} - loaded {sceneObjects.Count} scene objects");
                finished(sceneObjects);
            }
        }
    }
}
