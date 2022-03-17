using Fusion;
using UnityEngine;

namespace Systems.Network
{
    public class PlayerSpawner : MonoBehaviour
    {
        [SerializeField] private NetworkObject playerPrefab;

        private void Start()
        {
            NetworkSystem.Instance.OnPlayerJoinedEvent += PlayerJoined;
        }

        private void PlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log($"Player {player} spawn.");
            runner.Spawn(playerPrefab,null,null,player);
        }
    }
}