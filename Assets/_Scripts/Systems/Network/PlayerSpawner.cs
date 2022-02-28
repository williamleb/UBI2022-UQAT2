using Fusion;
using Units.Player;
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
            runner.Spawn(playerPrefab,null,null,player);
        }
    }
}