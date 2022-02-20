using System.Collections.Generic;
using Systems.Network;
using Fusion;
using UnityEngine;

namespace Dev.William
{
    public class TestSpawnPlayer : MonoBehaviour
    {
        [SerializeField] private TestPlayer playerPrefab;

        private readonly Dictionary<PlayerRef, TestPlayer> spawnedPlayers = new Dictionary<PlayerRef, TestPlayer>();
        
        public void Start()
        {
            NetworkSystem.Instance.OnPlayerJoinedEvent += PlayerJoined;
            NetworkSystem.Instance.OnPlayerLeftEvent += PlayerLeft;
        }

        private void OnDestroy()
        {
            if (NetworkSystem.HasInstance)
            {
                NetworkSystem.Instance.OnPlayerJoinedEvent -= PlayerJoined;
                NetworkSystem.Instance.OnPlayerLeftEvent -= PlayerLeft;
            }
        }

        private void PlayerJoined(NetworkRunner runner, PlayerRef playerRef)
        {
            if (!NetworkSystem.Instance.IsHost)
                return;
            
            if (!playerPrefab)
                return;

            var player = runner.Spawn(playerPrefab, Vector3.up, Quaternion.identity, playerRef);
            spawnedPlayers.Add(playerRef, player);
        }

        private void PlayerLeft(NetworkRunner runner, PlayerRef playerRef)
        {
            if (!NetworkSystem.Instance.IsHost)
                return;

            if (spawnedPlayers.TryGetValue(playerRef, out var playerObject))
            {
                runner.Despawn(playerObject.Object);
                spawnedPlayers.Remove(playerRef);
            }
        }
    }
}