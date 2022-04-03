using System;
using Fusion;
using Ingredients.Homework;
using Managers.Game;
using Systems.Settings;
using Systems.Teams;
using UnityEngine;

namespace Canvases.HUDs
{
    public class HUDScoreFeedback : NetworkBehaviour
    {
        private HUDSettings settings;

        private Team leftTeam;
        private Team rightTeam;

        [SerializeField] private Transform leftSpawnPoint;
        [SerializeField] private Transform rightSpawnPoint;

        private enum Direction
        {
            Left = 0,
            Right
        }

        private void Start()
        {
            settings = SettingsSystem.HUDSettings;

            if (GameManager.HasInstance)
            {
                GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
            }
        }

        private void OnGameStateChanged(GameState gameState)
        {
            if (gameState == GameState.Running)
            {
                if (TeamSystem.Instance.Teams.Count != 2)
                {
                    Debug.LogWarning("The game has more than two teams and the UI for two teams is activated. Therefore the UI will not be updated.");
                    return;
                }

                leftTeam = TeamSystem.Instance.Teams[0];

                if(leftTeam == null)
                {
                    Debug.LogWarning("Left team null, not spawning score feedback.");
                    return;
                }

                leftTeam.OnScoreIncrement += LeftTeamScoreChanged;
                leftTeam.OnScoreDecrement += LeftTeamScoreChanged;

                rightTeam = TeamSystem.Instance.Teams[1];

                if (rightTeam == null)
                {
                    Debug.LogWarning("Right team null, not spawning score feedback.");
                    return;
                }

                rightTeam.OnScoreIncrement += RightTeamScoreChanged;
                rightTeam.OnScoreDecrement += RightTeamScoreChanged;
            }
        }
    
        private void LeftTeamScoreChanged(int scoreValue)
        {
            RPC_SpawnVFX(Direction.Left, scoreValue);
        }

        private void RightTeamScoreChanged(int scoreValue)
        {
            RPC_SpawnVFX(Direction.Right, scoreValue);
        }

        [Rpc(RpcSources.StateAuthority,RpcTargets.All)]
        private void RPC_SpawnVFX(Direction direction, int scoreValue)
        {
            if (leftTeam == null || rightTeam == null || leftSpawnPoint == null || rightSpawnPoint == null)
            {
                Debug.LogWarning("Missing component to spawn score feedback.");
                return;
            }

            Transform transformToSpawn = direction == Direction.Left ? leftSpawnPoint : rightSpawnPoint;

            Debug.Log("Spawning Feedback for score value : " + scoreValue);
            if (scoreValue > 0)
            {
                if (HomeworkManager.HasInstance)
                {
                    var homeWorkDefinition = HomeworkManager.Instance.GetHomeworkDefinitionFromScoreValue(scoreValue);

                    if (homeWorkDefinition != null)
                    {
                        GameObject vfx = null;

                        if (homeWorkDefinition.Type.Equals(settings.TierOneScore.Definition.Type, StringComparison.OrdinalIgnoreCase))
                        {
                            vfx = direction == Direction.Left ? settings.TierOneScore.leftVfx : settings.TierOneScore.rightVfx;
                        }else if (homeWorkDefinition.Type.Equals(settings.TierTwoScore.Definition.Type, StringComparison.OrdinalIgnoreCase))
                        {
                            vfx = direction == Direction.Left ? settings.TierTwoScore.leftVfx : settings.TierTwoScore.rightVfx;
                        }else if (homeWorkDefinition.Type.Equals(settings.TierThreeScore.Definition.Type, StringComparison.OrdinalIgnoreCase))
                        {
                            vfx = direction == Direction.Left ? settings.TierThreeScore.leftVfx : settings.TierThreeScore.rightVfx;
                        }

                        Instantiate(vfx, transformToSpawn);
                    }
                }
            }
            else
            {
                var vfx = direction == Direction.Left ? settings.DescoreLeft : settings.DescoreRight;
                Instantiate(vfx, transformToSpawn);
            }
        }

        private void OnDestroy()
        {
            if (GameManager.HasInstance)
            {
                GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
            }

            if (leftTeam)
            {
                leftTeam.OnScoreIncrement -= LeftTeamScoreChanged;
                leftTeam.OnScoreDecrement -= LeftTeamScoreChanged;
            }

            if (rightTeam)
            {
                rightTeam.OnScoreIncrement -= RightTeamScoreChanged;
                rightTeam.OnScoreDecrement -= RightTeamScoreChanged;
            }
        }
    }
}
