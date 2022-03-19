using System.Linq;
using Fusion;
using Ingredients.Homework;
using Managers.Game;
using Sirenix.OdinInspector;
using Systems;
using Units.Player;
using UnityEngine;
using Utilities.Singleton;

namespace Managers.Score
{
    public class ScoreManager : Singleton<ScoreManager>
    {
        [SerializeField, Required] private NetworkObject scorePrefab;
        
        [SerializeField] private int scoreForLastHomework = 2; // TODO Replace with current phase info

        public void HandHomework(PlayerEntity playerEntity, HomeworkDefinition handedHomeworkDefinition)
        {
            if (GameManager.HasInstance && GameManager.Instance.CurrentState != GameState.Running)
                return;

            if(!TeamSystem.HasInstance)
            {
                Debug.LogWarning($"Tried to hand homework but TeamSystem doesn't exist");
                return;
            }

            var player = playerEntity.Object.InputAuthority;
            var team = TeamSystem.Instance.GetTeam(playerEntity.TeamId);
            if (!team) Debug.LogWarning($"Tried to hand homework for team {playerEntity.TeamId} which doesn't exist");
            
            if (GameManager.HasInstance && GameManager.Instance.IsNextHomeworkLastForPhase)
            {
                team.IncrementScore(scoreForLastHomework);
                playerEntity.PlayerScore += scoreForLastHomework;
            }
            else
            {
                team.IncrementScore(handedHomeworkDefinition.Points);
                playerEntity.PlayerScore += handedHomeworkDefinition.Points;
            }
            
            if (GameManager.HasInstance)
                GameManager.Instance.IncrementHomeworksGivenForPhase();
        }

        public void DecrementScore(PlayerEntity playerEntity, int numberOfPointsToLose)
        {
            if (GameManager.HasInstance && GameManager.Instance.CurrentState != GameState.Running)
                return;

            var team = TeamSystem.Instance.GetTeam(playerEntity.TeamId);
            if (!team) Debug.LogWarning($"Tried to remove for team {playerEntity.TeamId} which doesn't exist");

            team.DecrementScore(numberOfPointsToLose);
            playerEntity.PlayerScore -= numberOfPointsToLose;
        }

        public PlayerEntity FindPlayerWithHighestScore()
        {
            PlayerEntity playerRefWithHighestScore = null;
            int highestScore = int.MinValue;

            foreach (PlayerEntity playerEntity in PlayerSystem.Instance.AllPlayers)
            {
                if (playerEntity.PlayerScore > highestScore)
                {
                    highestScore = playerEntity.PlayerScore;
                    playerRefWithHighestScore = playerEntity;
                }
            }

            return playerRefWithHighestScore;
        }

        public Team FindTeamWithHighestScore()
        {
            if (!TeamSystem.HasInstance)
                return null;

            var teams = TeamSystem.Instance.Teams;
            var highestScoreTeam = teams.First();
            foreach (Team team in teams)
            {
                if (highestScoreTeam.ScoreValue > team.ScoreValue)
                {
                    highestScoreTeam = team;
                }
            }

            return highestScoreTeam;
        }

        private void Start()
        {
            if (GameManager.HasInstance)
            {
                GameManager.Instance.OnReset += OnReset;
            }
        }
        
        protected override void OnDestroy()
        {
            if (GameManager.HasInstance)
            {
                GameManager.Instance.OnReset -= OnReset;
            }

            base.OnDestroy();
        }

        private void OnReset()
        {
            foreach (Team team in TeamSystem.Instance.Teams)
            {
                team.ResetScore();
            }
        }
    }
}