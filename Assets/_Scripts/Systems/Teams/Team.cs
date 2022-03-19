using Fusion;
using Managers.Score;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Systems;
using Systems.Network;
using Systems.Settings;
using Units.Player;
using UnityEngine;

public class Team : NetworkBehaviour
{
    public static event Action<Team> OnTeamSpawned;
    public static event Action<Team> OnTeamDespawned;
    public event Action<int> OnScoreChanged;

    private TeamSettings teamSettings;
    private TeamSystem teamSystem;
    //Dictionary<PlayerRef, score>
    private Dictionary<PlayerRef, int> PlayersRefAndScore = new Dictionary<PlayerRef, int>();

    public int PlayerCount => PlayersRefAndScore.Count;

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
    public void AddPlayer(PlayerEntity playerEntity)
    {
        if (PlayersRefAndScore.Count <= teamSettings.MaxPlayerPerTeam)
        {
            var playerRef = playerEntity.Object.InputAuthority;

            if (PlayersRefAndScore.ContainsKey(playerRef))
            {
                PlayersRefAndScore[playerRef] = 0;
            }
            else
            {
                PlayersRefAndScore.Add(playerRef, 0);
            }

            playerEntity.TeamId = TeamId;
        }
        else
        {
            Debug.LogError($"Cannot add the player to the team {Name} because the team is full");
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RemovePlayer(PlayerRef playerRef)
    {
        if (PlayersRefAndScore.ContainsKey(playerRef))
        {
            PlayersRefAndScore.Remove(playerRef);
        }
    }

    public bool ContainPlayer(PlayerRef playerRef)
    {
        return PlayersRefAndScore.ContainsKey(playerRef);
    }

    public int GetScoreForPlayer(PlayerRef player)
    {
        if (PlayersRefAndScore.TryGetValue(player, out int score))
            return score;
        else
            return -1;
    }

    public (PlayerRef playerRef, int score) GetPlayerWithHighestScore()
    {
        int highestScore = int.MinValue;
        PlayerRef playerWithHighestScore = PlayerRef.None;

        foreach (PlayerRef playerRef in PlayersRefAndScore.Keys.ToList())
        {
            if (PlayersRefAndScore[playerRef] > highestScore)
            {
                highestScore = PlayersRefAndScore[playerRef];
                playerWithHighestScore = playerRef;
            }
        }

        return (playerWithHighestScore, highestScore);
    }
    
    public void IncrementScore(PlayerRef player, int scoreToAdd)
    {
        if (!NetworkSystem.Instance.IsHost)
            return;

        ScoreValue += scoreToAdd;
        IncrementPlayerScore(player, scoreToAdd);

        Debug.Log($"Player {player.PlayerId} score for team {TeamId}! Current player score : {PlayersRefAndScore[player]}");
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void IncrementPlayerScore(PlayerRef player, int scoreToAdd)
    {
        if (!PlayersRefAndScore.ContainsKey(player))
            PlayersRefAndScore.Add(player, scoreToAdd);
        else
            PlayersRefAndScore[player] += scoreToAdd;
    }

    public void DecrementScore(PlayerRef player, int scoreToRemove)
    {
        if (!NetworkSystem.Instance.IsHost)
            return;

        ScoreValue = Math.Max(0, ScoreValue - scoreToRemove);
        DecrementPlayerScore(player, scoreToRemove);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void DecrementPlayerScore(PlayerRef player, int scoreToRemove)
    {
        if (!PlayersRefAndScore.ContainsKey(player))
            PlayersRefAndScore[player] -= scoreToRemove;
        else
            PlayersRefAndScore[player] = Math.Max(0, PlayersRefAndScore[player] - scoreToRemove);
    }

    public void ResetScore()
    {
        if (!NetworkSystem.Instance.IsHost)
            return;

        ScoreValue = 0;
        ResetPlayersScore();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void ResetPlayersScore()
    {
        foreach (int playerRef in PlayersRefAndScore.Keys.ToList())
        {
            PlayersRefAndScore[playerRef] = 0;
        }
    }

    public void ClearTeam()
    {
        if (!NetworkSystem.Instance.IsHost)
            return;

        ScoreValue = 0;
        ClearPlayerAndScore();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void ClearPlayerAndScore()
    {
        PlayersRefAndScore.Clear();
    }

    private static void OnValueChanged(Changed<Team> changed)
    {
        var team = changed.Behaviour;
        team.OnScoreChanged?.Invoke(team.ScoreValue);
    }
}
