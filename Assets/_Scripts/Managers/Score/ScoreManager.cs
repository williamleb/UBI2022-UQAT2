using System;
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
        public static event Action<Team> OnTeamScoreChanged;

        public void HandHomework(PlayerEntity playerEntity, HomeworkDefinition handedHomeworkDefinition)
        {
            if (GameManager.HasInstance && (GameManager.Instance.CurrentState == GameState.NotStarted || GameManager.Instance.CurrentState == GameState.Finished))
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

            OnTeamScoreChanged?.Invoke(team);
        }

        public void DecrementScore(PlayerEntity playerEntity, int numberOfPointsToLose)
        {
            if (GameManager.HasInstance && (GameManager.Instance.CurrentState == GameState.NotStarted || GameManager.Instance.CurrentState == GameState.Finished))
                return;

            var team = TeamSystem.Instance.GetTeam(playerEntity.TeamId);
            if (!team) Debug.LogWarning($"Tried to remove for team {playerEntity.TeamId} which doesn't exist");

            team.DecrementScore(numberOfPointsToLose);
            playerEntity.PlayerScore -= numberOfPointsToLose;

            OnTeamScoreChanged?.Invoke(team);
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

        public bool AreScoresEqual()
        {
            if (TeamSystem.Instance.Teams.Count != 2)
            {
                Debug.LogWarning("Cannot verify if scores are equal as there is not exactly two team.");
                return false;
            }

            return TeamSystem.Instance.Teams[0].ScoreValue == TeamSystem.Instance.Teams[1].ScoreValue;
        }

        public Team FindTeamWithHighestScore()
        {
            if (!TeamSystem.HasInstance)
                return null;

            var teams = TeamSystem.Instance.Teams;
            var highestScoreTeam = teams.First();
            foreach (Team team in teams)
            {
                if (team.ScoreValue > highestScoreTeam.ScoreValue)
                {
                    highestScoreTeam = team;
                }
            }

            return highestScoreTeam;
        }

        public bool CanLosingTeamEqualize()
        {
            if (HomeworkManager.HasInstance)
            {
                if (TeamSystem.Instance.Teams.Count != 2)
                {
                    Debug.LogWarning("Cannot verify if losing team can equalize as there is not exactly two team.");
                    return false;
                }
                
                int scoreDiff = Math.Abs(TeamSystem.Instance.Teams[0].ScoreValue - TeamSystem.Instance.Teams[1].ScoreValue);
                int possibleScore = 0;

                foreach (Homework homework in HomeworkManager.Instance.Homeworks)
                {
                    if (!homework.IsFree)
                    {
                        possibleScore += HomeworkManager.Instance.GetScoreValueForHomeworkType(homework.Type);
                    }
                }

                return possibleScore >= scoreDiff;
            }

            Debug.LogWarning("No HomeworkManager in scene. Cannot check if the losing team can equalize.");
            return false;
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