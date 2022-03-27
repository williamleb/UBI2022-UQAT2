using Fusion;
using UnityEngine;

namespace Systems.Network
{
    public partial class NetworkSystem
    {
        public static float DeltaTime => Instance.NetworkRunner ? Instance.NetworkRunner.DeltaTime : 0f;
        public bool IsConnected => NetworkRunner != null;

        public bool IsHost => IsConnected && (NetworkRunner.GameMode == GameMode.Host || NetworkRunner.GameMode == GameMode.Single);
        public bool IsClient => IsConnected && NetworkRunner.GameMode == GameMode.Client;
        
        public NetworkObject Spawn(
            NetworkObject prefab, 
            Vector3? position = null, 
            Quaternion? rotation = null, 
            PlayerRef? inputAuthority = null,
            NetworkRunner.OnBeforeSpawned onBeforeSpawned = null,
            NetworkObjectPredictionKey? networkObjectPredictionKey = null)
        {
            return NetworkRunner.Spawn(prefab, position, rotation, inputAuthority, onBeforeSpawned, networkObjectPredictionKey);
        }

        public void Despawn(NetworkObject networkObject, bool allowPredicted = false)
        {
            NetworkRunner.Despawn(networkObject, allowPredicted);
        }
        
        public NetworkObject FindObject(NetworkId id)
        {
            NetworkRunner.TryFindObject(id, out var foundObject);
            return foundObject;
        }
    }
}