using Systems.Network;
using Fusion;
using UnityEngine;
using Utilities.Singleton;

namespace Units.AI
{
    public class AIManager : Singleton<AIManager>
    {
        [SerializeField] private NetworkObject entityPrefab;
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

        private void OnPlayerJoined(PlayerRef playerRef)
        {
            if (NetworkSystem.Instance.IsHost)
            {
                SpawnEntity();
            }
        }

        private void SpawnEntity()
        {
            var entity = NetworkSystem.Instance.Spawn(entityPrefab, Vector3.zero, Quaternion.identity).GetComponent<AIEntity>();
            entity.AddBrain(brainPrefab);
        }
    }
}