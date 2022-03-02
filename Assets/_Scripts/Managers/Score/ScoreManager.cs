﻿using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Managers.Game;
using Sirenix.OdinInspector;
using Systems.Network;
using Units.Player;
using UnityEngine;
using Utilities.Singleton;

namespace Managers.Score
{
    public class ScoreManager : Singleton<ScoreManager>
    {
        public event Action<Score, PlayerRef> OnScoreRegistered; 
        public event Action<PlayerRef> OnScoreUnregistered;

        [SerializeField, Required] private NetworkObject scorePrefab;
        
        [SerializeField] private int scoreForLastHomework = 2; // TODO Replace with current phase info
        
        private Dictionary<PlayerRef, Score> scores = new Dictionary<PlayerRef, Score>();

        public Score GetScoreForPlayer(PlayerRef player)
        {
            if (!scores.ContainsKey(player))
                return null;

            return scores[player];
        }

        public PlayerRef FindPlayerWithHighestScore()
        {
            if (!scores.Any())
                return PlayerRef.None;

            var highestScore = scores.First().Key;
            foreach (var player in scores.Keys)
            {
                if (scores[player].Value > scores[highestScore].Value)
                {
                    highestScore = player;
                }
            }
            
            return highestScore;
        }

        public void RegisterScore(Score score, PlayerRef player)
        {
            Debug.Assert(!scores.ContainsKey(player), $"Trying to register a score for player {player.PlayerId} when a score is already registered for them");
            scores.Add(player, score);
            
            OnScoreRegistered?.Invoke(score, player);
        }

        public void UnregisterScore(PlayerRef player)
        {
            scores.Remove(player);
            
            OnScoreUnregistered?.Invoke(player);
        }

        private void Start()
        {
            if (GameManager.HasInstance)
            {
                GameManager.Instance.OnReset += OnReset;
            }

            PlayerEntity.OnPlayerSpawned += OnPlayerSpawned;
            PlayerEntity.OnPlayerDespawned += OnPlayerDespawned;
        }

        private void OnPlayerSpawned(NetworkObject player)
        {
            if (!NetworkSystem.Instance.IsHost)
                return;
            
            if (!scorePrefab)
                return;
            
            NetworkSystem.Instance.Spawn(scorePrefab, Vector3.zero, Quaternion.identity, player.InputAuthority);
        }

        private void OnPlayerDespawned(NetworkObject player)
        {
            if (!NetworkSystem.Instance.IsHost)
                return;

            var score = GetScoreForPlayer(player.InputAuthority);
            NetworkSystem.Instance.Despawn(score.Object);
        }
        
        private void OnReset()
        {
            foreach (var score in scores.Values)
            {
                score.Reset();
            }
        }

        protected override void OnDestroy()
        {
            if (GameManager.HasInstance)
            {
                GameManager.Instance.OnReset -= OnReset;
            }

            PlayerEntity.OnPlayerSpawned -= OnPlayerSpawned;
            PlayerEntity.OnPlayerDespawned -= OnPlayerDespawned;
            
            base.OnDestroy();
        }

        public void HandHomework(PlayerEntity playerEntity)
        {
            if (GameManager.HasInstance && GameManager.Instance.CurrentState != GameState.Running)
                return;
            
            // TODO Manage teams (add points to all team)
            // TODO Manage different types of homework (fake, golden)

            var player = playerEntity.Object.InputAuthority;
            var score = GetScoreForPlayer(player);
            if (!score) Debug.LogWarning($"Tried to hand homework for player {player.PlayerId} which doesn't have any score");

            if (GameManager.HasInstance && GameManager.Instance.IsNextHomeworkLastForPhase)
            {
                score.Add(scoreForLastHomework);
            }
            else
            {
                score.Add(1);
            }
            
            if (GameManager.HasInstance)
                GameManager.Instance.IncrementHomeworksGivenForPhase();
        }
    }
}