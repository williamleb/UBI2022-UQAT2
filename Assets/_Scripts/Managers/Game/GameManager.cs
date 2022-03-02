using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Systems.Network;
using Units.Player;
using UnityEngine;
using Utilities.Singleton;

namespace Managers.Game
{
    public enum  GameState { NotStarted, Running, Finished }

    [RequireComponent(typeof(NetworkedGameData))]
    public class GameManager : Singleton<GameManager>
    {
        // TODO Handle different phases with a behavior specified in scriptable objects
        // Each phase has:
        // Team or not
        // Number of homework to hand in for the phase to finish
        // Area to unlock
        // Number of points for last homework

        public event Action OnBeginSpawn; // Only called on host
        public event Action OnEndSpawn; // Only called on host
        public event Action<Score, PlayerRef> OnScoreRegistered; 
        public event Action<PlayerRef> OnScoreUnregistered; 
        public event Action<GameState> OnGameStateChanged; 
        public event Action OnPhaseTotalHomeworkChanged
        {
            add => networkedData.OnPhaseTotalHomeworkChanged += value;
            remove => networkedData.OnPhaseTotalHomeworkChanged -= value;
        }
        
        [SerializeField] private int totalNumberOfHomeworkToFinishPhase = 10; // TODO Replace with current phase info
        [SerializeField] private int scoreForLastHomework = 2; // TODO Replace with current phase info
        
        private NetworkedGameData networkedData;
        private GameState currentState;

        private Coroutine spawnAndStartGameCoroutine = null;
        private HashSet<MonoBehaviour> spawnLocks = new HashSet<MonoBehaviour>();

        public int PhaseTotalHomework => networkedData.PhaseTotalHomework;
        public int NumberOfHomeworkToFinishPhase => totalNumberOfHomeworkToFinishPhase;
        public GameState CurrentState => currentState;
        public bool IsSpawning => spawnAndStartGameCoroutine != null;

        private Dictionary<PlayerRef, Score> scores = new Dictionary<PlayerRef, Score>();

        public bool IsRunning => currentState == GameState.Running;

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

        protected override void Awake()
        {
            base.Awake();

            networkedData = GetComponent<NetworkedGameData>();
        }
        
        private void Start()
        {
            networkedData.OnGameStateChanged += HandleGameStateChanged;
            
            NetworkSystem.Instance.OnPlayerJoinedEvent += OnPlayerJoined;
        }

        protected override void OnDestroy()
        {
            networkedData.OnGameStateChanged -= HandleGameStateChanged;
            
            if (NetworkSystem.HasInstance)
                NetworkSystem.Instance.OnPlayerJoinedEvent -= OnPlayerJoined;

            if (spawnAndStartGameCoroutine != null)
            {
                StopCoroutine(spawnAndStartGameCoroutine);
                spawnAndStartGameCoroutine = null;
            }
            
            base.OnDestroy();
        }

        private void HandleGameStateChanged(GameState newGameState)
        {
            if (newGameState == currentState)
                return;

            currentState = newGameState;
            OnGameStateChanged?.Invoke(currentState);
        }
        
        public void HandHomework(PlayerEntity playerEntity)
        {
            if (currentState != GameState.Running)
                return;
            
            // TODO Manage teams (add points to all team)
            // TODO Manage different types of homework (fake, golden)

            var player = playerEntity.Object.InputAuthority;
            var score = GetScoreForPlayer(player);
            if (!score) Debug.LogWarning($"Tried to hand homework for player {player.PlayerId} which doesn't have any score");

            ++networkedData.PhaseTotalHomework;
            if (PhaseTotalHomework == NumberOfHomeworkToFinishPhase)
            {
                score.Add(scoreForLastHomework);
                EndGame(); // TODO Switch phase if still has phases
            }
            else
            {
                score.Add(1);
            }
        }

        private void OnPlayerJoined(NetworkRunner runner, PlayerRef playerRef)
        {
            // TODO Remove this method once we have a good start condition not only based on the first player
            // being spawned
            if (currentState == GameState.Running || IsSpawning)
                return;


            if (NetworkSystem.Instance.IsHost)
                spawnAndStartGameCoroutine = StartCoroutine(SpawnAndStartGameRoutine());
        }

        public void LockSpawn(MonoBehaviour spawnLock)
        {
            spawnLocks.Add(spawnLock);
        }

        public void UnlockSpawn(MonoBehaviour spawnLock)
        {
            spawnLocks.Remove(spawnLock);
        }

        private IEnumerator SpawnAndStartGameRoutine()
        {
            spawnLocks.Clear();
            yield return null;
            
            OnBeginSpawn?.Invoke();
            yield return null;

            yield return new WaitUntil(() => spawnLocks.Count <= 0);
            
            spawnAndStartGameCoroutine = null;
            OnEndSpawn?.Invoke();
            
            StartGame();
        }
        
        public void StartGame()
        {
            if (currentState == GameState.Running)
                return;
            
            Reset();
            networkedData.GameIsStarted = true;
        }

        private void EndGame()
        {
            networkedData.GameIsEnded = true;
        }

        public void Reset()
        {
            networkedData.Reset();
            
            foreach (var score in scores.Values)
            {
                score.Reset();
            }
        }
    }
}