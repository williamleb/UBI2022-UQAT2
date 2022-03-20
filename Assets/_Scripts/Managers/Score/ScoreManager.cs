using System.Linq;
using Ingredients.Homework;
using Managers.Game;
using Systems;
using Systems.Teams;
using Units.Player;
using UnityEngine;
using Utilities.Singleton;

namespace Managers.Score
{
    public class ScoreManager : Singleton<ScoreManager>
    {
        public void HandHomework(PlayerEntity playerEntity, HomeworkDefinition handedHomeworkDefinition)
        {
            if (GameManager.HasInstance && GameManager.Instance.CurrentState != GameState.Running)
                return;

            if(!TeamSystem.HasInstance)
            {
                Debug.LogWarning($"Tried to hand homework but TeamSystem doesn't exist");
                return;
            }

            var team = TeamSystem.Instance.GetTeam(playerEntity.TeamId);
            if (!team) Debug.LogWarning($"Tried to hand homework for team {playerEntity.TeamId} which doesn't exist");
            
            team.IncrementScore(handedHomeworkDefinition.Points);
            playerEntity.PlayerScore += handedHomeworkDefinition.Points;
            
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

            foreach (PlayerEntity playerEntity in PlayerSystem.Instance.AllPlayers)
            {
                playerEntity.PlayerScore = 0;
            }
        }
    }
}