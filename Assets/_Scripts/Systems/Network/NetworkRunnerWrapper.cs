using Fusion;
using UnityEngine;

namespace Systems.Network
{
    public partial class NetworkSystem
    {
        public static float DeltaTime => Instance.networkRunner ? Instance.networkRunner.DeltaTime : 0f;
        
        public bool IsConnected => networkRunner != null;

        public bool IsHost => IsConnected && networkRunner.GameMode == GameMode.Host;
        public bool IsClient => IsConnected && networkRunner.GameMode == GameMode.Client;
        
        public NetworkObject Spawn(
            NetworkObject prefab, 
            Vector3? position = null, 
            Quaternion? rotation = null, 
            PlayerRef? inputAuthority = null,
            NetworkRunner.OnBeforeSpawned onBeforeSpawned = null,
            NetworkObjectPredictionKey? networkObjectPredictionKey = null)
        {
            return networkRunner.Spawn(prefab, position, rotation, inputAuthority, onBeforeSpawned, networkObjectPredictionKey);
        }

        public void Despawn(NetworkObject networkObject, bool allowPredicted = false)
        {
            networkRunner.Despawn(networkObject, allowPredicted);
        }
        
        public NetworkObject FindObject(NetworkId id)
        {
            networkRunner.TryFindObject(id, out var foundObject);
            return foundObject;
        }
    }
}