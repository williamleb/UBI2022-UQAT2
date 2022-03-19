using Fusion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Systems;
using Systems.Network;
using Systems.Settings;
using Units.Player;
using UnityEngine;

public class Team : NetworkBehaviour, IEquatable<Team>
{
    public static event Action<Team> OnTeamSpawned;
    public static event Action<Team> OnTeamDespawned;
    public event Action<int> OnScoreChanged;

    private TeamSettings teamSettings;
    private TeamSystem teamSystem;

    //TODO The player list is NOT GUARANTEED to be up to date on clients
    //since RPCs are not part of the network state. 
    private List<PlayerRef> playerList = new List<PlayerRef>();

    public int PlayerCount => playerList.Count;
    public List<PlayerRef> PlayerList => playerList;

    [Networked] public string Name { get; private set; }
    [Networked] [Capacity(128)] public string TeamId { get; private set; }
    [Networked(OnChanged = nameof(OnValueChanged))] public int ScoreValue { get; private set; }

    public override async void Spawned()
    {
        teamSystem = TeamSystem.Instance;
        teamSettings = SettingsSystem.TeamSettings;

        if (NetworkSystem.Instance.IsHost)
        {
            TeamId = Guid.NewGuid().ToString();
            Name = teamSystem.GetRandomTeamName();
        }

        ScoreValue = 0;

        await Task.Delay(100);
        OnTeamSpawned?.Invoke(this);
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        OnTeamDespawned?.Invoke(this);
    }

    [Rpc(RpcSources.StateAuthority,RpcTargets.All)]
    public void RPC_AssignPlayer(PlayerEntity playerEntity)
    {
        var playerRef = playerEntity.Object.InputAuthority;

        if (!playerList.Contains(playerRef))
            playerList.Add(playerRef);
        else
            Debug.LogWarning($"Player {playerRef} was already assigned to team {TeamId}");

        playerEntity.TeamId = TeamId;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_RemovePlayer(PlayerRef playerRef)
    {
        if (playerList.Contains(playerRef))
        {
            playerList.Remove(playerRef);
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

        foreach(PlayerRef playerRef in playerList)
        {
            var playerEntity = PlayerSystem.Instance.GetPlayerEntity(playerRef);
           
            if(playerEntity.PlayerScore > highestScore)
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
    }

    public void DecrementScore(int scoreToRemove)
    {
        if (!NetworkSystem.Instance.IsHost)
            return;

        ScoreValue = Math.Max(0, ScoreValue - scoreToRemove);
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

        playerList.Clear();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_ClearPlayerList()
    {
        if (!NetworkSystem.Instance.IsHost)
            return;

        playerList.Clear();
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
