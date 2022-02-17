using System;
using Systems.Network;
using Fusion;
using UnityEngine;

namespace Dev.William
{
    public class TestSpawnPlayer : MonoBehaviour
    {
        [SerializeField] private NetworkObject playerPrefab;
        
        public void Start()
        {
            NetworkSystem.Instance.OnPlayerJoinedEvent += OnPlayerJoined;
        }

        private void OnDestroy()
        {
            if (NetworkSystem.HasInstance)
                NetworkSystem.Instance.OnPlayerJoinedEvent -= OnPlayerJoined;
        }

        private void OnPlayerJoined(PlayerRef playerRef)
        {
            if (!NetworkSystem.Instance.IsHost)
                return;
            
            if (!playerPrefab)
                return;

            NetworkSystem.Instance.Spawn(playerPrefab, Vector3.up, Quaternion.identity, playerRef);
        }
    }
}