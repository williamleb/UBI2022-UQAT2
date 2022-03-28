using System.Linq;
using Fusion;
using Action = System.Action;

namespace Canvases.Menu.End
{
    public class EndUINetworkData : NetworkBehaviour
    {
        public event Action OnNumberOfPlayersReadyToReplayChanged;
        public event Action OnReplaying;
        
        [Networked(OnChanged = nameof(OnPlayersReadyToReplayChanged)), Capacity(12)] 
        private NetworkArray<PlayerRef> PlayersReadyToReplay { get; }
        
        [Networked(OnChanged = nameof(OnIsReplayingChanged))] 
        public NetworkBool IsReplaying { get; set; }

        public void LocalToggleReadyToReplay(PlayerRef player)
        {
            RPC_ToggleReadyToReplay(player);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_ToggleReadyToReplay(PlayerRef player)
        {
            if (IsPlayerReadyToReplay(player))
            {
                HostRemovePlayerReady(player);
            }
            else
            {
                HostAddPlayerReady(player);
            }
        }
        
        public void Reset() // Can only be called on host
        {
            if (!Object.HasStateAuthority)
                return;
            
            for (var i = 0; i < PlayersReadyToReplay.Length; ++i)
            {
                PlayersReadyToReplay.Set(i, default);
            }

            IsReplaying = false;
        }

        private void HostRemovePlayerReady(PlayerRef player)
        {
            for (var i = 0; i < PlayersReadyToReplay.Length; ++i)
            {
                if (PlayersReadyToReplay[i] == player)
                {
                    PlayersReadyToReplay.Set(i, default);
                }
            }
        }

        private void HostAddPlayerReady(PlayerRef player)
        {
            for (var i = 0; i < PlayersReadyToReplay.Length; ++i)
            {
                if (PlayersReadyToReplay[i] == default)
                {
                    PlayersReadyToReplay.Set(i, player);
                    return;
                }
            }
        }

        public bool IsPlayerReadyToReplay(PlayerRef player)
        {
            return PlayersReadyToReplay.Contains(player);
        }

        private void UpdatePlayersReadyToReplay()
        {
            OnNumberOfPlayersReadyToReplayChanged?.Invoke();
        }

        private void UpdateIsReplaying()
        {
            if (IsReplaying)
            {
                OnReplaying?.Invoke();
            }
        }

        private static void OnPlayersReadyToReplayChanged(Changed<EndUINetworkData> changed)
        {
            changed.Behaviour.UpdatePlayersReadyToReplay();
        }

        private static void OnIsReplayingChanged(Changed<EndUINetworkData> changed)
        {
            changed.Behaviour.UpdateIsReplaying();
        }
    }
}