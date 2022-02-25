using System.Collections.Generic;
using Fusion;
using Managers.Game;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Canvases.HUDs
{
    [RequireComponent(typeof(CanvasGroup))]
    public class HUD : MonoBehaviour
    {
        [Header("HUD Scores")]
        [SerializeField, Required] private RectTransform hudScoreContainer;
        [SerializeField, Required] private GameObject hudScorePrefab;
        
        private CanvasGroup canvasGroup;

        private Dictionary<PlayerRef, HUDScore> hudScores = new Dictionary<PlayerRef, HUDScore>();

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        private void Start()
        {
            if (!GameManager.HasInstance)
                return;
            
            GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
            GameManager.Instance.OnScoreRegistered += OnScoreRegistered;
            GameManager.Instance.OnScoreUnregistered += OnScoreUnregistered;
        }

        private void OnDestroy()
        {
            if (!GameManager.HasInstance)
                return;
            
            GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
            GameManager.Instance.OnScoreRegistered -= OnScoreRegistered;
            GameManager.Instance.OnScoreUnregistered -= OnScoreUnregistered;
        }

        private void OnGameStateChanged(GameState newGameState)
        {
            canvasGroup.alpha = newGameState == GameState.Running ? 1f : 0f;
        }

        private void OnScoreRegistered(Score score, PlayerRef player)
        {
            SpawnScore(score, player);
        }

        private void OnScoreUnregistered(PlayerRef player)
        {
            DestroyScore(player);
        }

        private void SpawnScore(Score score, PlayerRef player)
        {
            var hudScoreGameObject = Instantiate(hudScorePrefab, hudScoreContainer);
            hudScoreGameObject.name = $"HUDScore-Player{player.PlayerId}";
            
            var hudScore = hudScoreGameObject.GetComponent<HUDScore>();
            Debug.Assert(hudScore);
            
            hudScore.Init(score);
            hudScores.Add(player, hudScore);
            
            RedrawScoresLayout();
        }

        private void DestroyScore(PlayerRef player)
        {
            Debug.Assert(hudScores.ContainsKey(player));
            var hudScore = hudScores[player];
            
            hudScores.Remove(player);
            Destroy(hudScore.gameObject);
            
            RedrawScoresLayout();
        }

        private void RedrawScoresLayout()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(hudScoreContainer);
        }
    }
}