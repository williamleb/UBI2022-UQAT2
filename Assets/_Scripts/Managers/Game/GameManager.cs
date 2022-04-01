using Managers.Score;
using System;
using System.Collections;
using System.Collections.Generic;
using Systems;
using Systems.Level;
using Systems.MapGeneration;
using Systems.Network;
using Systems.Settings;
using Systems.Teams;
using UnityEngine;
using Utilities.Singleton;

namespace Managers.Game
{
    public enum  GameState { NotStarted, Running, Overtime, Finished }

    [RequireComponent(typeof(NetworkedGameData))]
    public class GameManager : Singleton<GameManager>
    {
        public event Action OnBeginSpawn; // Only called on host
        public event Action OnEndSpawn; // Only called on host
        public event Action OnReset; // Only called on host
        public event Action OnBeginDespawn; // Only called on host
        public event Action<GameState> OnGameStateChanged; 
        public event Action OnPhaseTotalHomeworkChanged
        {
            add => networkedData.OnPhaseTotalHomeworkChanged += value;
            remove => networkedData.OnPhaseTotalHomeworkChanged -= value;
        }

        private GameSettings settings;
        private GameTimer gameTimer;
        private NetworkedGameData networkedData;
        private GameState currentState;
        private bool endGameOnScore;
        //At overtime only (null otherwise), this variable contains the favorite team.
        //By default, the favorite team is the team that had the highest score at the end of the last game.
        private Team overTimeFavoriteTeam = null;

        private Coroutine spawnAndStartGameCoroutine;
        private readonly HashSet<MonoBehaviour> spawnLocks = new HashSet<MonoBehaviour>();

        public int HomeworksNeededToFinishGame => settings.NumberOfHomeworksToFinishGame;
        public GameTimer GameTimer => gameTimer;
        public GameState CurrentState => currentState;
        public bool IsSpawning => spawnAndStartGameCoroutine != null;
        public float GameProgression => Math.Max(GameProgressionFromTime, GameProgressionFromScore); // Between 0 (start of game) and 1 (end of game)
        public float GameProgressionFromTime => 1 - (gameTimer.RemainingTime / gameTimer.InitialDuration); 
        public float GameProgressionFromScore => ScoreManager.HasInstance ? ScoreManager.Instance.FindTeamWithHighestScore().ScoreValue / (float) settings.NumberOfHomeworksToFinishGame : 0f; 

        public bool IsRunning => currentState == GameState.Running;
        public bool IsOvertime => currentState == GameState.Overtime;

        protected override void Awake()
        {
            base.Awake();

            settings = SettingsSystem.GameSettings;
            networkedData = GetComponent<NetworkedGameData>();
            gameTimer = GetComponent<GameTimer>();
            
        }
        
        private void Start()
        {

            networkedData.OnGameStateChanged += HandleGameStateChanged;
            gameTimer.OnTimerExpired += EndGame;
            LevelSystem.Instance.OnGameLoad += OnGameLoaded;
            ScoreManager.OnTeamScoreChanged += OnTeamScoreChanged;
        }

        protected override void OnDestroy()
        {
            networkedData.OnGameStateChanged -= HandleGameStateChanged;
            gameTimer.OnTimerExpired -= EndGame;

            if (LevelSystem.HasInstance)
            {
                LevelSystem.Instance.OnGameLoad -= OnGameLoaded;
            }

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

        private void OnGameLoaded()
        {
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
            if (MapGenerationSystem.HasInstance)
                MapGenerationSystem.Instance.GenerateMap();
            
            yield return null;
            
            spawnLocks.Clear();
            yield return null;
            
            OnBeginSpawn?.Invoke();
            yield return null;

            yield return new WaitUntil(() => spawnLocks.Count <= 0);
            
            spawnAndStartGameCoroutine = null;
            OnEndSpawn?.Invoke();
            
            StartGame();
            PlayerSystem.Instance.SetPlayersPositionToSpawn();
        }
        
        public void StartGame()
        {
            if (currentState == GameState.Running || currentState == GameState.Overtime)
                return;
            
            Reset();
            networkedData.GameIsStarted = true;
            gameTimer.Init(settings.GameDurationInSeconds);
        }

        public void EndGame()
        {
            if (!NetworkSystem.Instance.IsHost)
                return;

            if (currentState  == GameState.NotStarted || currentState == GameState.Finished)
                return;

            if (currentState == GameState.Running)
            {
                if (CanOvertime())
                {
                    overTimeFavoriteTeam = ScoreManager.Instance.FindTeamWithHighestScore();
                    networkedData.GameIsOvertime = true;
                    gameTimer.Init(settings.OvertimeDurationInSeconds);
                }
                else
                {
                    networkedData.GameIsEnded = true;
                }
            }
            else
            {
                networkedData.GameIsEnded = true;
            }
        }

        public void CleanUpAndReturnToLobby()
        {
            OnBeginDespawn?.Invoke();
            
            if (MapGenerationSystem.HasInstance)
                MapGenerationSystem.Instance.CleanUpMap();
            
            LevelSystem.Instance.LoadLobby();
        }

        private bool CanOvertime() 
        {
            if (!settings.EnableOvertime)
                return false;

            if (TeamSystem.Instance.Teams.Count != 2)
            {
                Debug.LogWarning("Overtime only works for two team games.");
                return false;
            }

            if (ScoreManager.HasInstance)
            {
                if (TeamSystem.Instance.Teams[0].ScoreValue == TeamSystem.Instance.Teams[1].ScoreValue)
                {
                    endGameOnScore = true;
                    Debug.Log("Both teams have the same score, overtime starts.");
                    return true;
                }

                if (ScoreManager.Instance.CanLosingTeamEqualize())
                {
                    Debug.Log("Losing team can equalize, overtime starts.");
                    return true;
                }

                return false;
            }

            Debug.LogWarning("No scoremanager in scene, overtime deactivated.");
            return false;
        }

        public void Reset()
        {
            networkedData.Reset();

            endGameOnScore = false;
            overTimeFavoriteTeam = null;

            OnReset?.Invoke();
        }

        public void OnTeamScoreChanged(Team team)
        {
            if (currentState == GameState.Overtime)
            {
                if (endGameOnScore)
                {
                    EndGame();
                }

                if (overTimeFavoriteTeam != null &&  team.Equals(overTimeFavoriteTeam))
                {
                    EndGame();
                }
            }

            if (currentState == GameState.Running && team.ScoreValue >= settings.NumberOfHomeworksToFinishGame)
            {
                EndGame();
            }
        }
    }
}