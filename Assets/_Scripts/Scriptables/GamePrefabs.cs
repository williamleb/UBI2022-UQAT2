using Fusion;
using UnityEngine;

namespace Scriptables
{
    [CreateAssetMenu(fileName = "Prefabs", menuName = "Game/Prefabs")]
    public class GamePrefabs : ScriptableObject
    {
        [SerializeField] private NetworkObject playerPrefab;

        public NetworkObject PlayerPrefab => playerPrefab;
    }
}