using System.Collections.Generic;
using System.Linq;
using Fusion;
using Systems.Level;
using UnityEngine;

namespace Systems.Network
{
    public class LoadMainMenuOnServerShutdown : MonoBehaviour
    {
        [SerializeField] private List<ShutdownReason> shutdownReasonsFilter = new List<ShutdownReason>();

        private void Start()
        {
            NetworkSystem.Instance.OnShutdownEvent += OnServerShutdown;
        }

        private void OnDestroy()
        {
            if (NetworkSystem.HasInstance)
                NetworkSystem.Instance.OnShutdownEvent -= OnServerShutdown;
        }

        private void OnServerShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            if (shutdownReasonsFilter.Any() && !shutdownReasonsFilter.Contains(shutdownReason))
                return;
            
            if (LevelSystem.HasInstance)
                LevelSystem.Instance.LoadMainMenu();
        }
    }
}