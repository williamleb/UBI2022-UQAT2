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

                var playerWithHighestScore = ScoreManager.HasInstance
                    ? ScoreManager.Instance.FindPlayerWithHighestScore()
                    : PlayerRef.None;
                
                winnerText.Text = $"Winner: Player{playerWithHighestScore.PlayerId}";
            }
        }

        private void OnRestart()
        {
            RPC_RestartGame();
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_RestartGame()
        {
            if (GameManager.HasInstance)
                GameManager.Instance.StartGame();
        }
    }
}