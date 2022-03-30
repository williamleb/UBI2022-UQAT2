using System;
using Fusion;
using UnityEngine;

namespace Managers.Lobby
{
    public class ReadyUpNetworkData : NetworkBehaviour
    {
        public event Action OnStartingChanged;
        public event Action OnTimeChanged;
        public event Action OnNumberOfDotsChanged;
        
        [Networked(OnChanged = nameof(NetworkOnStartingChanged))] public NetworkBool IsStarting { get; set; }
        [Networked(OnChanged = nameof(NetworkOnTimeChanged))] public int Time { get; set; }
        [Networked(OnChanged = nameof(NetworkOnNumberOfDotsChanged))] public int NumberOfDots { get; set; }

        public override void Spawned()
        {
            base.Spawned();
            Debug.Log("Spawned");
        }

        public void Revert(int maxTime)
        {
            IsStarting = false;
            Time = maxTime;
            NumberOfDots = 0;
        }

        private static void NetworkOnStartingChanged(Changed<ReadyUpNetworkData> changed)
        {
            changed.Behaviour.OnStartingChanged?.Invoke();
        }
        
        private static void NetworkOnTimeChanged(Changed<ReadyUpNetworkData> changed)
        {
            changed.Behaviour.OnTimeChanged?.Invoke();
        }
        
        private static void NetworkOnNumberOfDotsChanged(Changed<ReadyUpNetworkData> changed)
        {
            changed.Behaviour.OnNumberOfDotsChanged?.Invoke();
        }
    }
}