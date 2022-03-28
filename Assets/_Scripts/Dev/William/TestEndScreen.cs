using Canvases.Components;
using Fusion;
using Managers.Game;
using Managers.Score;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Dev.William
{
    public class TestEndScreen : NetworkBehaviour
    {
        [SerializeField, Required] private ButtonUIComponent restartButton;
        [SerializeField, Required] private TextUIComponent winnerText;

        private void Start()
        {
            restartButton.OnClick += OnRestart;

            if (GameManager.HasInstance)
                GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
        }

        private void OnDestroy()
        {
            restartButton.OnClick -= OnRestart;
            
            if (GameManager.HasInstance)
                GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
        }

        private void OnGameStateChanged(GameState gameState)
        {
            if (gameState == GameState.Finished)
            {
                restartButton.Select();

                if (ScoreManager.HasInstance)
                {
                    if (ScoreManager.Instance.AreScoresEqual())
                    {
                        winnerText.Text = $"It's a draw!";
                    }
                    else
                    {
                        var teamWithHighestScore = ScoreManager.Instance.FindTeamWithHighestScore();
                        winnerText.Text = $"Winner: Team {(teamWithHighestScore ? teamWithHighestScore.Name : "null")}";
                    }
                }                
            }
        }

        private void OnRestart()
        {
            if (!GameManager.HasInstance)
                return;
            
            if (Object.HasStateAuthority)
                GameManager.Instance.CleanUpAndReturnToLobby();
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_RestartGame()
        {
            if (GameManager.HasInstance)
                GameManager.Instance.StartGame();
        }
    }
}