using System;
using Fusion;

namespace Systems.MapGeneration
{
    [Serializable]
    public struct MapReference
    {
        public MapGenerationInfo MapGenerationInfo;
        public NetworkObject Hallways;
        public NetworkObject PropPrefab;
        public NetworkObject SpawnPoints;
    }
}