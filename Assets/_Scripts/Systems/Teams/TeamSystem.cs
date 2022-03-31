using Fusion;
using Scriptables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Systems.Level;
using Systems.Network;
using Systems.Settings;
using Units.Player;
using UnityEngine;
using Utilities.Extensions;
using Utilities.Singleton;
using Random = UnityEngine.Random;

namespace Systems.Teams
{
    public class TeamSystem : PersistentSingleton<TeamSystem>
    {
        private const string PREFABS_FOLDER_PATH = "Game";

        public static event Action OnTeamCreated;

        private TeamSettings teamSettings;
        private GamePrefabs prefabs;
        private bool areTeamsCreated = false;

        private readonly Dictionary<string, Team> teams = new Dictionary<string, Team>();
        public List<Team> Teams => teams.Values.ToList();

        public void RegisterTeam(Team team)
        {
            Debug.Log($"Registering team {team.Name}");
            teams.Add(team.TeamId, team);
        }

        public void UnregisterTeam(Team team)
        {
            if (teams.ContainsKey(team.TeamId))
            {
                teams.Remove(team.TeamId);
            }
        }

        protected override void Awake()
        {
            base.Awake();

            teamSettings = SettingsSystem.TeamSettings;
            LoadPrefabs();

            if (!NetworkSystem.Instance.IsHost)
                return;

            CreateTeams();
            PlayerEntity.OnPlayerDespawned += OnPlayerDespawned;
        }
        
        private void OnEnable()
        {
            LevelSystem.Instance.OnMainMenuStartLoad += OnReturnToMainMenu;
        }

        private void OnDisable()
        {
            if (LevelSystem.HasInstance)
            {
                LevelSystem.Instance.OnMainMenuStartLoad -= OnReturnToMainMenu;
            }
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
                    var teamNetworkObject = NetworkSystem.Instance
                        .Spawn(prefabs.TeamPrefab, Vector3.zero, Quaternion.identity).GetBehaviour<Team>();

                    Debug.Log($"Team created with id {teamNetworkObject.TeamId} and name {teamNetworkObject.Name}");
                }
            }
            else
            {
                Debug.LogWarning("Warning, cannot create teams as teams are already created.");
            }

            areTeamsCreated = true;
            OnTeamCreated?.Invoke();
        }

        public Team AssignTeam(PlayerEntity playerEntity, string teamId = default)
        {
            if (!NetworkSystem.Instance.IsHost)
                return null;

            if (!teams.Any() || !areTeamsCreated)
                CreateTeams();

            if (!string.IsNullOrEmpty(playerEntity.TeamId))
                return AssignNewTeam(playerEntity);

            if (string.IsNullOrEmpty(teamId))
                return AssignFirstSmallestTeam(playerEntity);

            Team team = GetTeam(teamId);

            if (team != null)
            {
                team.RPC_AssignPlayer(playerEntity);
                Debug.Log(
                    $"Team {team.Name} assigned to player {playerEntity.Object.InputAuthority}. [From AssignTeam with specified teamId.]");
                return team;
            }
            else
            {
                Debug.LogError($"Error assigning player to team {teamId}. Assigning random team.");
                return AssignFirstSmallestTeam(playerEntity);
            }
        }

        private Team AssignFirstSmallestTeam(PlayerEntity playerEntity)
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

            playerEntity.TeamId = smallestTeam.TeamId;
            smallestTeam.RPC_AssignPlayer(playerEntity);

            Debug.Log(
                $"Team {smallestTeam.Name} assigned to player {playerEntity.Object.InputAuthority}. [From AssignFirstSmallestTeam]");

            return smallestTeam;
        }

        //Cycle through all teams so as to make a predictable team change.
        private Team AssignNewTeam(PlayerEntity playerEntity)
        {
            if (Teams.Count <= 1)
            {
                Debug.LogWarning("Cannot change teams since there is only one team ");
                return null;
            }

            int teamIndex = GetTeamIndex(playerEntity.TeamId);
            Team currentTeam = GetTeam(playerEntity.TeamId);

            if (currentTeam != null)
                currentTeam.RPC_RemovePlayer(playerEntity.Object.InputAuthority);

            if (teamIndex == -1)
            {
                Debug.Log("Could not find the team index, assigning the first smallest team.");
                return AssignFirstSmallestTeam(playerEntity);
            }

            Team newTeam;

            if (teamIndex + 1 < Teams.Count)
                newTeam = Teams[teamIndex + 1];
            else
                newTeam = Teams[0];

            newTeam.RPC_AssignPlayer(playerEntity);
            playerEntity.TeamId = newTeam.TeamId;

            Debug.Log($"Team {newTeam.Name} assigned to player {playerEntity.Object.InputAuthority}");

            return newTeam;
        }

        private int GetTeamIndex(string teamId)
        {
            for (int i = 0; i < Teams.Count; i++)
                if (Teams[i].TeamId.Equals(teamId))
                    return i;

            return -1;
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
            StringBuilder stringBuilder = new StringBuilder();

            do
            {
                stringBuilder.Append(teamSettings.TeamAdjectives[Random.Range(0, teamSettings.TeamAdjectives.Length)]
                    .Capitalize());
                stringBuilder.Append(" ");
                stringBuilder.Append(teamSettings.TeamPronouns[Random.Range(0, teamSettings.TeamPronouns.Length)]);
            } while (string.IsNullOrEmpty(stringBuilder.ToString()) && IsNameAlreadyUsed(stringBuilder.ToString()));

            return stringBuilder.ToString();
        }

        private void LoadPrefabs()
        {
            var prefabResources = Resources.LoadAll<GamePrefabs>(PREFABS_FOLDER_PATH);

            Debug.Assert(prefabResources.Any(),
                $"An object of type {nameof(GamePrefabs)} should be in the folder {PREFABS_FOLDER_PATH}");
            if (prefabResources.Length > 1)
                Debug.LogWarning(
                    $"More than one object of type {nameof(GamePrefabs)} was found in the folder {PREFABS_FOLDER_PATH}. Taking the first one.");

            prefabs = prefabResources.First();
        }

        private void OnPlayerDespawned(NetworkObject networkObject)
        {
            if (!NetworkSystem.HasInstance || !NetworkSystem.Instance.IsHost)
                return;

            var playerEntity = PlayerSystem.Instance.GetPlayerEntity(networkObject.InputAuthority);

            if (playerEntity && !string.IsNullOrEmpty(playerEntity.TeamId))
            {
                GetTeam(playerEntity.TeamId).RPC_RemovePlayer(networkObject.InputAuthority);
            }
            else
            {
                foreach (Team team in Teams)
                {
                    if (team.ContainPlayer(networkObject.InputAuthority))
                    {
                        team.RPC_RemovePlayer(networkObject.InputAuthority);
                    }
                }
            }
        }

        private void OnReturnToMainMenu()
        {
            teams.Clear();
        }
    }
}