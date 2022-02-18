using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utilities.Singleton;

namespace Systems.Network
{
    public partial class NetworkSystem : PersistentSingleton<NetworkSystem>, INetworkRunnerCallbacks
    {
        private NetworkRunner networkRunner;
        
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

        //TODO: to remove.
        private void OnGUI()
        {
            if (networkRunner == null)
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
                GUI.TextField(new Rect(10, 10, 40, 20), networkRunner.GameMode == GameMode.Host ? "Host" : "Client");
            }
        }

        async void StartGame(GameMode mode)
        {
            networkRunner = gameObject.AddComponent<NetworkRunner>();
            networkRunner.ProvideInput = true;

            await networkRunner.StartGame(new StartGameArgs
            {
                GameMode = mode,
                Scene = SceneManager.GetActiveScene().buildIndex,
                SceneObjectProvider = gameObject.AddComponent<NetworkSceneManagerDefault>()
            });
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