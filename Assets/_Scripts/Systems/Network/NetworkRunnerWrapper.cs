﻿using System;
using BehaviorDesigner.Runtime.Tasks.Unity.Timeline;
using Fusion;
using UnityEngine;
using Utilities.Event;
using Utilities.Singleton;

namespace Systems.Network
{
    public partial class NetworkSystem : PersistentSingleton<NetworkSystem>, INetworkRunnerCallbacks
    {
        public static float DeltaTime => Instance.runner ? Instance.runner.DeltaTime : 0f;
        
        [NonSerialized] public MemoryEvent<PlayerRef> OnPlayerJoinedEvent;
        public event Action<PlayerRef> OnPlayerLeftEvent;
        
        public bool IsConnected => runner != null;

        public bool IsHost => IsConnected && runner.GameMode == GameMode.Host;
        public bool IsClient => IsConnected && runner.GameMode == GameMode.Client;

        public NetworkObject Spawn(
            NetworkObject prefab, 
            Vector3? position = null, 
            Quaternion? rotation = null, 
            PlayerRef? inputAuthority = null,
            NetworkRunner.OnBeforeSpawned onBeforeSpawned = null,
            NetworkObjectPredictionKey? networkObjectPredictionKey = null)
        {
            return runner.Spawn(prefab, position, rotation, inputAuthority, onBeforeSpawned, networkObjectPredictionKey);
        }

        public void Despawn(NetworkObject networkObject, bool allowPredicted = false)
        {
            runner.Despawn(networkObject, allowPredicted);
        }

        public bool IsPlayer(int playerId)
        {
            return runner != null && runner.LocalPlayer.PlayerId == playerId;
        }

        public bool IsPlayer(PlayerRef playerRef)
        {
            return IsPlayer(playerRef.PlayerId);
        }

        public NetworkObject FindObject(NetworkId id)
        {
            runner.TryFindObject(id, out var foundObject);
            return foundObject;
        }
    }
}