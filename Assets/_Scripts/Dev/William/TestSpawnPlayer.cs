using System;
using System.Collections.Generic;
using Systems.Network;
using Fusion;
using UnityEngine;

namespace Dev.William
{
    public class TestSpawnPlayer : MonoBehaviour
    {
        [SerializeField] private NetworkObject playerPrefab;

        private readonly Dictionary<PlayerRef, NetworkObject> spawnedPlayers = new Dictionary<PlayerRef, NetworkObject>();
        
        public void Start()
        {
            NetworkSystem.Instance.OnPlayerJoinedEvent += OnPlayerJoined;
            NetworkSystem.Instance.OnPlayerLeftEvent += OnPlayerJoined;
        }

        private void OnDestroy()
        {
            if (NetworkSystem.HasInstance)
            {
                NetworkSystem.Instance.OnPlayerJoinedEvent -= OnPlayerJoined;
                NetworkSystem.Instance.OnPlayerLeftEvent -= OnPlayerLeft;
            }
        }

        private void OnPlayerJoined(PlayerRef playerRef)
        {
            if (!NetworkSystem.Instance.IsHost)
                return;
            
            if (!playerPrefab)
                return;

            var player = NetworkSystem.Instance.Spawn(playerPrefab, Vector3.up, Quaternion.identity, playerRef);
            spawnedPlayers.Add(playerRef, player);
        }

        private void OnPlayerLeft(PlayerRef playerRef)
        {
            if (!NetworkSystem.Instance.IsHost)
                return;

            if (spawnedPlayers.TryGetValue(playerRef, out var playerObject))
            {
                NetworkSystem.Instance.Despawn(playerObject);
                spawnedPlayers.Remove(playerRef);
            }
        }
    }
}