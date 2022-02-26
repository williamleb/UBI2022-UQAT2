using System;
using Canvases.Components;
using Fusion;
using Managers.Game;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Canvases.HUDs
{
    public class HUDScore : MonoBehaviour
    {
        [SerializeField, Required] private TextUIComponent scoreText;
        [SerializeField, Required] private TextUIComponent playerIdText;
        
        private Score score;

        public void Init(Score scoreToLookAt)
        {
            Debug.Assert(scoreToLookAt);
            score = scoreToLookAt;

            scoreText.Text = $"{score.Value}";
            playerIdText.Text = $"{score.Object.InputAuthority.PlayerId}";

            score.OnScoreChanged += OnScoreChanged;
        }

        private void OnDestroy()
        {
            if (score)
                score.OnScoreChanged -= OnScoreChanged;
        }

        private void OnScoreChanged(int newScore)
        {
            // TODO Cute animation
            scoreText.Text = $"{newScore}";
        }
    }
}