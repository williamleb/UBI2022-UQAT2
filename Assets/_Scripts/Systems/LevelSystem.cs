using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Scriptables;
using Systems.Network;
using Trisibo;
using Units.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Utilities.Event;
using Utilities.Singleton;

namespace Systems
{
    public class LevelSystem : PersistentSingleton<LevelSystem>
    {
        private const string SCENES_FOLDER_PATH = "Game";
        
        public MemoryEvent OnLobbyLoad;
        public MemoryEvent OnGameLoad;
        
        [SerializeField, FormerlySerializedAs("mainMenuSceneIndex")] private SceneField mainMenuOverride;
        [SerializeField, FormerlySerializedAs("lobbySceneIndex")] private SceneField lobbyOverride;
        [SerializeField, FormerlySerializedAs("gameSceneIndex")] private SceneField gameOverride;

        private GameScenes scenes;
        private Scene loadedScene;

        public INetworkSceneObjectProvider NetworkSceneObjectProvider { get; private set; }
        public LevelState State { get; private set; }
        public int ActiveSceneIndex { get; private set; }

        private SceneField MainMenuScene => mainMenuOverride ?? scenes.MainMenu;
        private SceneField LobbyScene => lobbyOverride ?? scenes.Lobby;
        private SceneField GameScene => gameOverride ?? scenes.Game;

        public enum LevelState
        {
            TRANSITION,
            LOBBY,
            GAME
        }

        protected override void Awake()
        {
            base.Awake();
            
            LoadScenes();
        }

        private void LoadScenes()
        {
            var sceneResources = Resources.LoadAll<GameScenes>(SCENES_FOLDER_PATH);

            Debug.Assert(sceneResources.Any(), $"An object of type {nameof(GameScenes)} should be in the folder {SCENES_FOLDER_PATH}");
            if (sceneResources.Length > 1)
                Debug.LogWarning($"More than one object of type {nameof(GameScenes)} was found in the folder {SCENES_FOLDER_PATH}. Taking the first one.");

            scenes = sceneResources.First();
        }

        public void Start()
        {
            NetworkSceneObjectProvider = gameObject.AddComponent<NetworkSceneManager>();
        }

        // Since the NetworkRunner is deleted after a connection error (idk why),
        // called by the runner to re-register actions
        public void SubscribeNetworkEvents()
        {
            NetworkSystem.Instance.OnSceneLoadDoneEvent += ChangeLevelState;
        }
        
        public void LoadLobby()
        {
            State = LevelState.TRANSITION;

            Debug.Log("Loading lobby scene.");
            ActiveSceneIndex = LobbyScene.BuildIndex;
            NetworkSystem.Instance.NetworkRunner.SetActiveScene(LobbyScene.BuildIndex);
        }

        public void LoadGame()
        {
            State = LevelState.TRANSITION;

            Debug.Log($"Loading scene with index {GameScene.BuildIndex}");
            ActiveSceneIndex = GameScene.BuildIndex;
            NetworkSystem.Instance.NetworkRunner.SetActiveScene(GameScene.BuildIndex);
        }
        
        private void ChangeLevelState(NetworkRunner networkRunner)
        {
            OnLobbyLoad.ClearMemory();
            OnGameLoad.ClearMemory();
            
            if (ActiveSceneIndex == LobbyScene.BuildIndex)
            {
                Debug.Log("Invoking spawn player");
                State = LevelState.LOBBY;
                OnLobbyLoad.InvokeWithMemory();
            }
            else if (ActiveSceneIndex == GameScene.BuildIndex || NetworkSystem.Instance.DebugMode)
            {
                State = LevelState.GAME;
                OnGameLoad.InvokeWithMemory();
            }
            else
            {
                State = LevelState.TRANSITION;
            }

            PlayerInputHandler.FetchInput = true;
        }

        public class NetworkSceneManager : NetworkSceneManagerBase
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
