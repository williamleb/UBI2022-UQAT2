using Systems.Network;
using Fusion;
using UnityEngine;
using Utilities.Singleton;

namespace Units.AI
{
    public class AIManager : Singleton<AIManager>
    {
        [SerializeField] private AIEntity entityPrefab;
        [SerializeField] private GameObject brainPrefab;

        private void Start()
        {
            NetworkSystem.Instance.OnPlayerJoinedEvent += OnPlayerJoined;
        }
        
        protected override void OnDestroy()
        {
            if (NetworkSystem.HasInstance)
            {
                NetworkSystem.Instance.OnPlayerJoinedEvent -= OnPlayerJoined;
            }
            
            base.OnDestroy();
        }

        private void OnPlayerJoined(NetworkRunner runner, PlayerRef playerRef)
        {
            if (NetworkSystem.Instance.IsHost)
            {
                var entity = runner.Spawn(entityPrefab, Vector3.zero, Quaternion.identity,playerRef);
                entity.AddBrain(brainPrefab);
            }
        }
    }
}