using Fusion;
using UnityEngine;

namespace Scriptables
{
    [CreateAssetMenu(fileName = "Prefabs", menuName = "Game/Prefabs")]
    public class GamePrefabs : ScriptableObject
    {
        [SerializeField] private NetworkObject playerPrefab;
        [SerializeField] private NetworkObject teamPrefab;

        public NetworkObject PlayerPrefab => playerPrefab;
        public NetworkObject TeamPrefab => teamPrefab;

    }
}