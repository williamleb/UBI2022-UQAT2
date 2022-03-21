using System.Collections.Generic;
using Managers.Game;
using Managers.Score;
using Sirenix.OdinInspector;
using Systems.Teams;
using UnityEngine;
using UnityEngine.UI;

namespace Canvases.HUDs
{
    public class HUD : MonoBehaviour
    {
        [Header("HUD Scores")]
        [SerializeField, Required] private RectTransform hudScoreContainer;
        [SerializeField, Required] private GameObject hudScorePrefab;

        //Dictionary<TeamId, HUDScore>
        private readonly Dictionary<string, HUDScore> hudScores = new Dictionary<string, HUDScore>();

        private void Start()
        {
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

        private void OnDestroy()
        {
            if (!ScoreManager.HasInstance)
                return;

            Team.OnTeamSpawned -= OnTeamSpawn;
            Team.OnTeamDespawned -= OnTeamDespawned;

            if (GameManager.HasInstance)
            {
                GameManager.Instance.OnBeginSpawn -= SpawnAllUnspawnedScoreHud;
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
            SpawnScoreHud(team);
        }

        private void OnTeamDespawned(Team team)
        {
            DestroyScore(team);
        }

        private void SpawnScoreHud(Team team)
        {
            var hudScoreGameObject = Instantiate(hudScorePrefab, hudScoreContainer);
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
            LayoutRebuilder.ForceRebuildLayoutImmediate(hudScoreContainer);
        }
    }
}