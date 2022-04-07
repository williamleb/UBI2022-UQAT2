using Fusion;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Systems.Network;
using Systems.Settings;
using Units.Player;
using UnityEngine;

namespace Systems.Teams
{
    public partial class Team : NetworkBehaviour, IEquatable<Team>
    {
        public static event Action<Team> OnTeamSpawned;
        public static event Action<Team> OnTeamDespawned;
        public event Action<int> OnPlayerCountChanged;
        public event Action<int> OnScoreChanged;
        public event Action<int> OnScoreIncrement;
        public event Action<int> OnScoreDecrement;

        private TeamSettings teamSettings;
        private TeamSystem teamSystem;

        //TODO The player list is NOT GUARANTEED to be up to date on clients
        //since RPCs are not part of the network state. 
        private readonly List<PlayerRef> playerList = new List<PlayerRef>();

        [Networked] public int PlayerCount { get; private set; }
        public List<PlayerRef> PlayerList => playerList;

        [Networked] [Capacity(128)] public string TeamId { get; private set; }

        [Networked(OnChanged = nameof(OnValueChanged))]
        public int ScoreValue { get; private set; }

        public override async void Spawned()
        {
            teamSystem = TeamSystem.Instance;
            teamSettings = SettingsSystem.TeamSettings;

            if (NetworkSystem.Instance.IsHost)
            {
                TeamId = Guid.NewGuid().ToString();
            }

            CustomizationSpawned();
            gameObject.name = $"Team - {Name}";
            
            ScoreValue = 0;
            teamSystem.RegisterTeam(this);

            await Task.Delay(100);
            OnTeamSpawned?.Invoke(this);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            teamSystem.UnregisterTeam(this);
            OnTeamDespawned?.Invoke(this);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_AssignPlayer(PlayerEntity playerEntity)
        {
            var playerRef = playerEntity.Object.InputAuthority;

            if (!playerList.Contains(playerRef))
            {
                playerList.Add(playerRef);
                PlayerCount = playerList.Count;
            }
                
            else
                Debug.Log($"Player {playerRef} was already assigned to team {TeamId}");

            playerEntity.TeamId = TeamId;
            OnPlayerCountChanged?.Invoke(playerList.Count);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_RemovePlayer(PlayerEntity playerEntity)
        {
            var playerRef = playerEntity.Object.InputAuthority;

            if (playerList.Contains(playerRef))
            {
                playerList.Remove(playerRef);
                PlayerCount = playerList.Count;
                OnPlayerCountChanged?.Invoke(playerList.Count);
            }
        }

        public bool ContainPlayer(PlayerRef playerRef)
        {
            return playerList.Contains(playerRef);
        }

        public PlayerEntity GetPlayerWithHighestScore()
        {
            PlayerEntity teamPlayerWithHighestScore = null;
            int highestScore = int.MinValue;

            foreach (PlayerRef playerRef in playerList)
            {
                var playerEntity = PlayerSystem.Instance.GetPlayerEntity(playerRef);

                if (playerEntity.PlayerScore > highestScore)
                {
                    teamPlayerWithHighestScore = playerEntity;
                    highestScore = playerEntity.PlayerScore;
                }
            }

            return teamPlayerWithHighestScore;
        }

        public void IncrementScore(int scoreToAdd)
        {
            if (!NetworkSystem.Instance.IsHost)
                return;

            ScoreValue += scoreToAdd;
            RPC_IncrementScore(scoreToAdd);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_IncrementScore(int scoreToAdd)
        {
            OnScoreIncrement?.Invoke(scoreToAdd);
        }

        public void DecrementScore(int scoreToRemove)
        {
            if (!NetworkSystem.Instance.IsHost)
                return;

            ScoreValue = Math.Max(0, ScoreValue - scoreToRemove);
            RPC_DecrementScore(scoreToRemove);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_DecrementScore(int scoreToRemove)
        {
            OnScoreDecrement?.Invoke(-scoreToRemove);
        }

        public void ResetScore()
        {
            if (!NetworkSystem.Instance.IsHost)
                return;

            ScoreValue = 0;
        }

        public void ClearPlayerList()
        {
            if (!NetworkSystem.Instance.IsHost)
                return;

            RPC_ClearPlayerList();
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_ClearPlayerList()
        {
            playerList.Clear();
            PlayerCount = playerList.Count;
            OnPlayerCountChanged?.Invoke(playerList.Count);
        }

        private static void OnValueChanged(Changed<Team> changed)
        {
            var team = changed.Behaviour;
            team.OnScoreChanged?.Invoke(team.ScoreValue);
        }

        public bool Equals(Team other)
        {
            return other is Team team &&
                   base.Equals(other) &&
                   TeamId == team.TeamId;
        }
    }
}