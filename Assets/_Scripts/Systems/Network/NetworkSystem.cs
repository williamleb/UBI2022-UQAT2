using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utilities.Singleton;

public class NetworkSystem : PersistentSingleton<NetworkSystem>, INetworkRunnerCallbacks
{
    private NetworkRunner runner;
    //[SerializeField] private NetworkPrefabRef playerPrefab;
    //private Dictionary<PlayerRef, NetworkObject> spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();

    //TODO: to remove.
    private void OnGUI()
    {
        if (runner == null)
        {
            if (GUI.Button(new Rect(0, 0, 200, 40), "Host"))
            {
                StartGame(GameMode.Host);
            }
            if (GUI.Button(new Rect(0, 40, 200, 40), "Join"))
            {
                StartGame(GameMode.Client);
            }
        }
        else
        {
            GUI.TextField(new Rect(10, 10, 40, 20), runner.GameMode == GameMode.Host ? "Host" : "Client");
        }
    }

    async void StartGame(GameMode mode)
    {
        runner = gameObject.AddComponent<NetworkRunner>();
        runner.ProvideInput = true;

        await runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            Scene = SceneManager.GetActiveScene().buildIndex,
            SceneObjectProvider = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
    }

    public void OnConnectedToServer(NetworkRunner runner){}

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason){}

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token){}

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data){}

    public void OnDisconnectedFromServer(NetworkRunner runner){}

    public void OnInput(NetworkRunner runner, NetworkInput input){}

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input){}

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        // Spawn player prefab
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        // Find and remove the players prefab
        //if (spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
        //{
        //    runner.Despawn(networkObject);
        //    spawnedCharacters.Remove(player);
        //}
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data){}

    public void OnSceneLoadDone(NetworkRunner runner){}

    public void OnSceneLoadStart(NetworkRunner runner){}

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList){}

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason){}

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message){}

}
