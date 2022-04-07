using System;
using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityParticleSystem;
using Scriptables;
using Systems.Network;
using Trisibo;
using Units.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Utilities;
using Utilities.Event;
using Utilities.Singleton;

namespace Systems.Level
{
    public class LevelSystem : PersistentSingleton<LevelSystem>
    {
        private const string SCENES_FOLDER_PATH = "Game";

        private static int newId = 0;
        public int id;

        public static event Action OnMainMenuStartLoad;
        public static event Action OnLobbyStartLoad;
        public static event Action OnGameStartLoad;
        
        public static MemoryEvent OnMainMenuLoad;
        public static MemoryEvent OnLobbyLoad;
        public static MemoryEvent OnGameLoad;

        public event Action OnBeforeUnload;
        
        [SerializeField, FormerlySerializedAs("mainMenuSceneIndex")] private SceneField mainMenuOverride;
        [SerializeField, FormerlySerializedAs("lobbySceneIndex")] private SceneField lobbyOverride;
        [SerializeField, FormerlySerializedAs("gameSceneIndex")] private SceneField gameOverride;

        private GameScenes scenes;
        private Scene loadedScene;

        private NetworkSceneManager networkManager;

        public INetworkSceneObjectProvider NetworkSceneObjectProvider => networkManager;
        public LevelState State { get; private set; }
        public int ActiveSceneIndex { get; private set; }

        private SceneField MainMenuScene => mainMenuOverride == null || mainMenuOverride.BuildIndex == -1 ? scenes.MainMenu : mainMenuOverride;
        private SceneField LobbyScene => lobbyOverride == null || lobbyOverride.BuildIndex == -1 ? scenes.Lobby : lobbyOverride;
        private SceneField GameScene => gameOverride == null || gameOverride.BuildIndex == -1 ? scenes.Game : gameOverride;

        public bool IsMainMenu => ActiveSceneIndex == MainMenuScene.BuildIndex;
        public bool IsLobby => ActiveSceneIndex == LobbyScene.BuildIndex;
        public bool IsGame => ActiveSceneIndex == GameScene.BuildIndex;

        public enum LevelState
        {
            Transition,
            Lobby,
            Game,
            MainMenu
        }

        protected override void Awake()
        {
            base.Awake();

            id = newId;
            newId++;
            
            CreateNetworkManager();
            Debug.Log($"LevelSystem.Awake{id}=== Assign new active scene {SceneManager.GetActiveScene().buildIndex} (was {Instance.ActiveSceneIndex})");
            ActiveSceneIndex = SceneManager.GetActiveScene().buildIndex;
            LoadScenes();
            
            NetworkSystem.OnSceneLoadDoneEvent += OnSceneLoadDone;
            ChangeLevelState();
        }

        private void CreateNetworkManager()
        {
            if (networkManager)
            {
                Destroy(networkManager);
            }
            networkManager = gameObject.AddComponent<NetworkSceneManager>();
        }
            

        private void LoadScenes()
        {
            var sceneResources = Resources.LoadAll<GameScenes>(SCENES_FOLDER_PATH);

            Debug.Assert(sceneResources.Any(), $"An object of type {nameof(GameScenes)} should be in the folder {SCENES_FOLDER_PATH}");
            if (sceneResources.Length > 1)
                Debug.LogWarning($"More than one object of type {nameof(GameScenes)} was found in the folder {SCENES_FOLDER_PATH}. Taking the first one.");
 
            scenes = sceneResources.First();
        }

        // Since the NetworkRunner is deleted after a connection error (idk why),
        // called by the runner to re-register actions
        public void SubscribeNetworkEvents()
        {
            // Debug.Log($"LevelSystem.SubscribeNetworkEvents{id}===");
            // NetworkSystem.OnSceneLoadDoneEvent += OnSceneLoadDone;
            //
            // // Necessary because the network system even may have already been played
            // ChangeLevelState();
        }
        public void UnsubscribeNetworkEvents()
        {
            // Debug.Log($"LevelSystem.UnsubscribeNetworkEvents{id}===");
            // NetworkSystem.OnSceneLoadDoneEvent -= OnSceneLoadDone;
        }
        
        public void LoadLobby()
        {
            State = LevelState.Transition;

            Debug.Log("Loading lobby scene.");
            Debug.Log($"LevelSystem.LoadLobby{id}=== Assign new active scene {LobbyScene.BuildIndex} (was {Instance.ActiveSceneIndex})");
            ActiveSceneIndex = LobbyScene.BuildIndex;
            OnBeforeUnload?.Invoke();
            ClearEventMemory();
            OnLobbyStartLoad?.Invoke();
            NetworkSystem.Instance.NetworkRunner.SetActiveScene(LobbyScene.BuildIndex);
        }

        public void LoadGame()
        {
            State = LevelState.Transition;

            Debug.Log($"Loading scene with index {GameScene.BuildIndex}");
            Debug.Log($"LevelSystem.LoadGame{id}=== Assign new active scene {GameScene.BuildIndex} (was {Instance.ActiveSceneIndex})");
            ActiveSceneIndex = GameScene.BuildIndex;
            OnBeforeUnload?.Invoke();
            ClearEventMemory();
            OnGameStartLoad?.Invoke();
            NetworkSystem.Instance.NetworkRunner.SetActiveScene(GameScene.BuildIndex);
        }

        public void LoadMainMenu()
        {
            if (State == LevelState.Transition)
            {
                Debug.LogWarning("Cannot load the main menu while in a scene transition.");
                return;
            }
            
            State = LevelState.Transition;
            OnBeforeUnload?.Invoke();
            ClearEventMemory();
            StartCoroutine(LoadMainMenuRoutine());
        }

        private IEnumerator LoadMainMenuRoutine()
        {
            Debug.Log($"LevelSystem.LoadMainMenuRoutine{id}=== Assign new active scene {MainMenuScene.BuildIndex} (was {Instance.ActiveSceneIndex})");
            ActiveSceneIndex = MainMenuScene.BuildIndex;
            OnMainMenuStartLoad?.Invoke();
            
            AsyncOperation unloadOperation;
            try
            {
                unloadOperation = SceneManager.UnloadSceneAsync(MainMenuScene.BuildIndex);
            }
            catch (ArgumentException)
            {
                // The scene was not loaded. Do nothing.
                unloadOperation = null;
            }
            if (unloadOperation != null)
            {
                yield return unloadOperation;
            }

            DontDestroyOnLoadUtils.DestroyAll((o => o != gameObject));
            
            yield return SceneManager.LoadSceneAsync(MainMenuScene.BuildIndex);
            DestroyImmediate(gameObject);
        }

        private void OnSceneLoadDone(NetworkRunner networkRunner)
        {
            Debug.Log($"OnSceneLoadDone{id}=== Destroy"); 
            ChangeLevelState();
        }

        private void OnDestroy()
        {
            Debug.Log($"LevelSystem{id}=== Destroy"); 
            NetworkSystem.OnSceneLoadDoneEvent -= OnSceneLoadDone; 
        }

        private void ChangeLevelState()
        {
            var truc = ActiveSceneIndex == LobbyScene.BuildIndex ? "Lobby" :
                ActiveSceneIndex == GameScene.BuildIndex ? "Game" :
                ActiveSceneIndex == MainMenuScene.BuildIndex ? "Main menu" : "Wat";
            Debug.Log($"LevelSystem{id}=== Loaded build index: {ActiveSceneIndex}, it is {truc}");

            var state = GetStateFromSceneIndex();
            if (State == state)
                return;

            ClearEventMemory();

            State = state;
            if (State == LevelState.Lobby)
            {
                Debug.Log("Invoking spawn player");
                OnLobbyLoad.InvokeWithMemory();
            }
            else if (State == LevelState.Game || NetworkSystem.Instance.DebugMode)
            {
                OnGameLoad.InvokeWithMemory();
            }
            else if (State == LevelState.MainMenu)
            {
                OnMainMenuLoad.InvokeWithMemory();
            }

            PlayerInputHandler.FetchInput = true;
        }

        private void ClearEventMemory()
        {
            OnLobbyLoad.ClearMemory();
            OnGameLoad.ClearMemory();
            OnMainMenuLoad.ClearMemory();
        }

        private LevelState GetStateFromSceneIndex()
        {
            if (ActiveSceneIndex == LobbyScene.BuildIndex)
            {
                return LevelState.Lobby;
            }

            if (ActiveSceneIndex == GameScene.BuildIndex || NetworkSystem.Instance.DebugMode)
            {
                return LevelState.Game;
            }

            if (ActiveSceneIndex == MainMenuScene.BuildIndex)
            {
                return LevelState.MainMenu;
            }

            return LevelState.Transition;
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
                
                Debug.Log($"LevelSystem.SwitchScene{Instance.id}=== Assign new active scene {(int) newScene} (was {Instance.ActiveSceneIndex})");
                Instance.ActiveSceneIndex = newScene;

                Debug.Log($"Switched Scene from {prevScene} to {newScene} - loaded {sceneObjects.Count} scene objects");
                finished(sceneObjects);
            }
        }
    }
}
