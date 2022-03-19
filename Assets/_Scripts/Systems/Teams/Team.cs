using Fusion;
using Managers.Score;
using System;
using System.Collections.Generic;
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

    private List<PlayerEntity> playerList { get; set;}
    private TeamSettings teamSettings;
    private TeamSystem teamSystem;

    public int PlayerCount => playerList.Count;

    [Networked] public string Name { get; private set; }
    [Networked] [Capacity(128)] public string TeamId { get; private set; }
    [Networked(OnChanged = nameof(OnValueChanged))] public int ScoreValue { get; private set; }
    [Networked] [Capacity(10)] public NetworkDictionary<PlayerRef, int> playerScore => default;

    public override async void Spawned()
    {
        teamSystem = TeamSystem.Instance;
        teamSettings = SettingsSystem.TeamSettings;

        playerList = new List<PlayerEntity>();

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
        if (!NetworkSystem.Instance.IsHost)
            return;

        if (playerList.Count <= teamSettings.MaxPlayerPerTeam)
        {
            playerList.Add(playerEntity);
            playerEntity.TeamId = TeamId;
        }
        else
        {
            Debug.LogError($"Cannot add the player to the team {Name} because the team is full");
        }
    }

    public int GetScoreForPlayer(PlayerRef player)
    {
        var score = playerScore;

        if (score.TryGet(player, out int pScore))
            return pScore;
        else
            return -1;
    }

    public void AddScore(PlayerRef player, int scoreToAdd)
    {
        if (!NetworkSystem.Instance.IsHost)
            return;

        //Weird fix. Saw that on the discord, no idea why.
        var score = playerScore;

        if (!score.ContainsKey(player))
            score.Add(player, scoreToAdd);
        else
            score[player] = score[player] + scoreToAdd;

        ScoreValue += scoreToAdd;

        Debug.Log($"Player {player} score for team {TeamId}! Current player score : {score[player]}");
    }

    public void RemoveScore(int scoreToRemove)
    {
        ScoreValue = Math.Max(0, ScoreValue - scoreToRemove);
    }

    public void ResetScore()
    {
        ScoreValue = 0;
    }

    private static void OnValueChanged(Changed<Team> changed)
    {
        var score = changed.Behaviour;
        score.OnScoreChanged?.Invoke(score.ScoreValue);
    }
}
