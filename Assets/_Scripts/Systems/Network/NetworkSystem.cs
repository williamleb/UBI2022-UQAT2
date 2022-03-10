using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utilities.Singleton;

namespace Systems.Network
{
    public partial class NetworkSystem : PersistentSingleton<NetworkSystem>, INetworkRunnerCallbacks
    {
        //TODO move lobby name out
        [SerializeField] private static string customLobbyName = "bababooeyLobby";
        
        public NetworkRunner NetworkRunner { get; private set; }
        public bool DebugMode { get; set; } = true;

        private void Start()
        {
            LevelSystem.Instance.SubscribeNetworkEvents();
            PlayerSystem.Instance.SubscribeNetworkEvents();
        }

        #region Events

        public event Action<NetworkRunner, NetworkInput> OnInputEvent;
        public event Action<NetworkRunner, PlayerRef, NetworkInput> OnInputMissingEvent;
        public event Action<NetworkRunner> OnConnectedToServerEvent;
        public event Action<NetworkRunner> OnDisconnectedFromServerEvent;
        public event Action<NetworkRunner, NetworkRunnerCallbackArgs.ConnectRequest, byte[]> OnConnectRequestEvent;
        public event Action<NetworkRunner, NetAddress, NetConnectFailedReason> OnConnectFailedEvent;
        public event Action<NetworkRunner, PlayerRef> OnPlayerJoinedEvent;
        public event Action<NetworkRunner, PlayerRef> OnPlayerLeftEvent;
        public event Action<NetworkRunner, SimulationMessagePtr> OnUserSimulationMessageEvent;
        public event Action<NetworkRunner, ShutdownReason> OnShutdownEvent;
        public event Action<NetworkRunner, List<SessionInfo>> OnSessionListUpdateEvent;
        public event Action<NetworkRunner, Dictionary<string, object>> OnCustomAuthenticationResponseEvent;
        public event Action<NetworkRunner> OnSceneLoadDoneEvent;
        public event Action<NetworkRunner> OnSceneLoadStartEvent;
        public event Action<NetworkRunner, PlayerRef, ArraySegment<byte>> OnReliableDataEvent;

        #endregion

        private void OnGUI()
        {
            if (DebugMode)
            {
                if (NetworkRunner == null)
                {
                    if (GUI.Button(new Rect(0, 0, 200, 40), "Host"))
                    {
                        StartGame(GameMode.Host);
                    }
                    if (GUI.Button(new Rect(0, 40, 200, 40), "Join"))
                    {
                        StartGame(GameMode.Client);
                    }
                    if (GUI.Button(new Rect(0, 80, 200, 40), "Single"))
                    {
                        StartGame(GameMode.Single);
                    }
                }
                else
                {
                    GUI.TextField(new Rect(10, 10, 40, 20), NetworkRunner.GameMode == GameMode.Host ? "Host" : "Client");
                }
            }
        }

        async void StartGame(GameMode mode)
        {
            NetworkRunner = gameObject.AddComponent<NetworkRunner>();
            NetworkRunner.ProvideInput = true;

            await NetworkRunner.StartGame(new StartGameArgs
            {
                GameMode = mode,
                Scene = SceneManager.GetActiveScene().buildIndex,
                SceneObjectProvider = gameObject.AddComponent<NetworkSceneManagerDefault>()
            });
        }

        //Create a hosted game with the given session name. 
        public async Task<bool> CreateGame(string sessionName)
        {
            Debug.Log($"Creating game with session name {sessionName}");

            NetworkRunner = gameObject.AddComponent<NetworkRunner>();
            NetworkRunner.ProvideInput = true;

            var result = await NetworkRunner.StartGame(new StartGameArgs()
            {
                SessionName = sessionName.ToLower(),
                CustomLobbyName = customLobbyName,
                GameMode = GameMode.Host,
                SceneObjectProvider = LevelSystem.Instance.NetworkSceneObjectProvider
            });

            if (result.Ok)
            {
                LevelSystem.Instance.LoadLobby();
                return true;
            }
            else
            {
                Debug.Log($"Failed to join game with session name : {sessionName}");
                return false;
            }        
        }

        //Try to join a game base on a specific session name.
        public async Task<bool> TryJoinGame(string sessionName)
        {
            Debug.Log($"Trying to join game with session name : {sessionName}");

            NetworkRunner = gameObject.AddComponent<NetworkRunner>();
            NetworkRunner.ProvideInput = true;

            var result = await NetworkRunner.StartGame(new StartGameArgs()
            {
                SessionName = sessionName.ToLower(),
                CustomLobbyName = customLobbyName,
                GameMode = GameMode.Client,
                SceneObjectProvider = LevelSystem.Instance.NetworkSceneObjectProvider,
                DisableClientSessionCreation = true
            });

            if (result.Ok)
            {
                Debug.Log($"Connected to session name : {sessionName}.");
                Debug.Log(NetworkRunner.SessionInfo.ToString());
                return true;
            }
            else
            {
                Debug.Log($"Failed to join game with session name : {sessionName}");
                return false;
            }
        }

        public void OnConnectedToServer(NetworkRunner runner) => OnConnectedToServerEvent?.Invoke(runner);
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) => OnConnectFailedEvent?.Invoke(runner,remoteAddress,reason);
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) => OnConnectRequestEvent?.Invoke(runner,request,token);
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) => OnCustomAuthenticationResponseEvent?.Invoke(runner,data);
        public void OnDisconnectedFromServer(NetworkRunner runner) => OnDisconnectedFromServerEvent?.Invoke(runner);
        public void OnInput(NetworkRunner runner, NetworkInput input) => OnInputEvent?.Invoke(runner,input);
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) => OnInputMissingEvent?.Invoke(runner,player,input);
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) => OnPlayerJoinedEvent?.Invoke(runner,player);
        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) => OnPlayerLeftEvent?.Invoke(runner,player);
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) => OnReliableDataEvent?.Invoke(runner,player,data);
        public void OnSceneLoadDone(NetworkRunner runner) => OnSceneLoadDoneEvent?.Invoke(runner);
        public void OnSceneLoadStart(NetworkRunner runner) => OnSceneLoadStartEvent?.Invoke(runner);
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) => OnSessionListUpdateEvent?.Invoke(runner,sessionList);
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) => OnShutdownEvent?.Invoke(runner,shutdownReason);
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) => OnUserSimulationMessageEvent?.Invoke(runner,message);
    }
}