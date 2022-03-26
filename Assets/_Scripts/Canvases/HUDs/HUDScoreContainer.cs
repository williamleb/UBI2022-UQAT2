using Canvases.HUDs;
using Managers.Game;
using Managers.Score;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using Systems.Teams;
using UnityEngine;
using UnityEngine.UI;

//Used only when the game has more than 2 teams
public class HUDScoreContainer : MonoBehaviour
{
    private RectTransform rectTransform;
    [SerializeField, Required] private GameObject hudScorePrefab;

    //Dictionary<TeamId, HUDScore>
    private readonly Dictionary<string, HUDScore> hudScores = new Dictionary<string, HUDScore>();

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        if (!ScoreManager.HasInstance)
            return;

        Team.OnTeamSpawned += OnTeamSpawn;
        Team.OnTeamDespawned += OnTeamDespawned;

        if (GameManager.HasInstance)
        {
            // If we came to the game scene from the lobby, we didn't notice the teams spawn, so this is necessary
            GameManager.Instance.OnBeginSpawn += SpawnAllUnspawnedScoreHud;
        }
    }

    private void SpawnAllUnspawnedScoreHud()
    {
        foreach (var team in TeamSystem.Instance.Teams)
        {
            if (!hudScores.ContainsKey(team.TeamId))
            {
                SpawnScoreHud(team);
            }
        }
    }

    private void OnTeamSpawn(Team team)
    {
        if (!hudScores.ContainsKey(team.TeamId))
        {
            SpawnScoreHud(team);
        }
    }

    private void OnTeamDespawned(Team team)
    {
        DestroyScore(team);
    }

    private void SpawnScoreHud(Team team)
    {
        var hudScoreGameObject = Instantiate(hudScorePrefab, rectTransform);
        hudScoreGameObject.name = $"HUDScore-TeamId{team.TeamId}";

        var hudScore = hudScoreGameObject.GetComponent<HUDScore>();
        Debug.Assert(hudScore);

        hudScore.Init(team);
        hudScores.Add(team.TeamId, hudScore);

        RedrawScoresLayout();
    }

    private void DestroyScore(Team team)
    {
        Debug.Assert(hudScores.ContainsKey(team.TeamId));
        var hudScore = hudScores[team.TeamId];

        hudScores.Remove(team.TeamId);

        if (hudScore)
            Destroy(hudScore.gameObject);

        RedrawScoresLayout();
    }

    private void RedrawScoresLayout()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    }
    private void OnDestroy()
    {
        Team.OnTeamSpawned -= OnTeamSpawn;
        Team.OnTeamDespawned -= OnTeamDespawned;

        if (GameManager.HasInstance)
        {
            GameManager.Instance.OnBeginSpawn -= SpawnAllUnspawnedScoreHud;
        }
    }
}
