using Scriptables;
using System;
using System.Collections.Generic;
using System.Linq;
using Systems;
using Systems.Network;
using Systems.Settings;
using Units.Player;
using UnityEngine;
using Utilities.Singleton;
using Random = UnityEngine.Random;

public class TeamSystem : PersistentSingleton<TeamSystem>
{
    public event Action<Team> OnTeamSpawned;
    public event Action<Team> OnTeamDespawned;

    private const string PREFABS_FOLDER_PATH = "Game";

    private TeamSettings teamSettings;
    private GamePrefabs prefabs;
    private bool areTeamsCreated = false;

    private Dictionary<string, Team> teams = new Dictionary<string, Team>();
    public List<Team> Teams => teams.Values.ToList();

    protected override void Awake()
    {
        base.Awake();

        teamSettings = SettingsSystem.TeamSettings;
        LoadPrefabs();

        if (!NetworkSystem.Instance.IsHost)
            return;
        
        CreateTeams();
    }

    private void CreateTeams()
    {
        if (!NetworkSystem.Instance.IsHost)
            return;

        if (!prefabs.TeamPrefab)
        {
            Debug.LogWarning("Missing team prefab in GamePrefabs scriptable object.");
            return;
        }

        if (areTeamsCreated)
            return;

        if (teams.Count == 0)
        {
            for (int i = 1; i <= teamSettings.NumberOfTeam; i++)
            {
                var teamNetworkObject = NetworkSystem.Instance.Spawn(prefabs.TeamPrefab, Vector3.zero, Quaternion.identity).GetBehaviour<Team>();
                teams.Add(teamNetworkObject.TeamId, teamNetworkObject);
                OnTeamSpawned?.Invoke(teamNetworkObject);

                Debug.Log($"Team created with id {teamNetworkObject.TeamId} and name {teamNetworkObject.Name}");
            }
        }else
        {
            Debug.LogWarning("Warning, cannot create teams as teams are already created.");
        }

        areTeamsCreated = true;
    }

    public void RemoveTeam(Team team)
    {
        OnTeamDespawned?.Invoke(team);
    }

    public Team AssignTeam(PlayerEntity player, string teamId = default)
    {
        if (!NetworkSystem.Instance.IsHost)
            return null;

        if (!teams.Any() || !areTeamsCreated)
            CreateTeams();

        if (string.IsNullOrEmpty(teamId))
            return AssignFirstSmallestTeam(player);

        Team team = GetTeam(teamId);

        if (team != null)
        {
            team.AddPlayer(player);
            Debug.Log($"Team {team.Name} assigned to player id {player.Id}");
            return team;
        }
        else
        {
            Debug.LogError($"Error assigning player to team {teamId}. Assigning random team.");
            return AssignFirstSmallestTeam(player);
        }
    }

    private Team AssignFirstSmallestTeam(PlayerEntity player)
    {
        Team smallestTeam = null;
        int currentLowestPlayerCount = int.MaxValue;

        foreach (Team team in teams.Values)
        {
            if (team.PlayerCount < currentLowestPlayerCount)
            {
                smallestTeam = team;
                currentLowestPlayerCount = team.PlayerCount;
            }
        }

        player.TeamId = smallestTeam.TeamId;
        smallestTeam.AddPlayer(player);
        Debug.Log($"Team {smallestTeam.Name} assigned to player id {player.Id}");
        return smallestTeam;
    }

    public Team GetTeam(string teamId)
    {
        if (string.IsNullOrEmpty(teamId))
        {
            Debug.LogError("Error. Invalid TeamId.");
            return null;
        }

        if (teams.TryGetValue(teamId, out Team team))
            return team;

        Debug.LogError($"Error. No team found with TeamId {teamId}");
        return null;
    }

    public bool IsNameAlreadyUsed(string teamName)
    {
        foreach (Team team in Teams)
            if (team.Name.Equals(teamName, StringComparison.OrdinalIgnoreCase))
                return true;

        return false;
    }

    public string GetRandomTeamName()
    {
        string name;
        int nameSearch = 1;

        do
        {
            name = teamSettings.TeamNames[Random.Range(0, teamSettings.TeamNames.Length)];
            
            nameSearch++;

            if(nameSearch >= teamSettings.TeamNames.Length){
                name = Random.Range(1,100).ToString();
                Debug.LogWarning("Warning, maximum team name search reached. Assigning random integer as team name.");
                break;
            }

        } while (string.IsNullOrEmpty(name) && IsNameAlreadyUsed(name));
        
        return name;
    }

    private void LoadPrefabs()
    {
        var prefabResources = Resources.LoadAll<GamePrefabs>(PREFABS_FOLDER_PATH);

        Debug.Assert(prefabResources.Any(), $"An object of type {nameof(GamePrefabs)} should be in the folder {PREFABS_FOLDER_PATH}");
        if (prefabResources.Length > 1)
            Debug.LogWarning($"More than one object of type {nameof(GamePrefabs)} was found in the folder {PREFABS_FOLDER_PATH}. Taking the first one.");

        prefabs = prefabResources.First();
    }
}
