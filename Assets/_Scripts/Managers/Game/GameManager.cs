using System;
using System.Collections;
using System.Collections.Generic;
using Systems;
using Systems.Network;
using Systems.Settings;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities.Singleton;

namespace Managers.Game
{
    public enum  GameState { NotStarted, Running, Finished }

    [RequireComponent(typeof(NetworkedGameData))]
    public class GameManager : Singleton<GameManager>
    {
        public event Action OnBeginSpawn; // Only called on host
        public event Action OnEndSpawn; // Only called on host
        public event Action OnReset; // Only called on host
        public event Action<GameState> OnGameStateChanged; 
        public event Action OnPhaseTotalHomeworkChanged
        {
            add => networkedData.OnPhaseTotalHomeworkChanged += value;
            remove => networkedData.OnPhaseTotalHomeworkChanged -= value;
        }

        private GameSettings settings;
        
        private NetworkedGameData networkedData;
        private GameState currentState;

        private Coroutine spawnAndStartGameCoroutine = null;
        private readonly HashSet<MonoBehaviour> spawnLocks = new HashSet<MonoBehaviour>();

        public int HomeworksHanded => networkedData.PhaseTotalHomework;
        public int HomeworksNeededToFinishGame => settings.NumberOfHomeworksToFinishGame;
        public GameState CurrentState => currentState;
        public bool IsSpawning => spawnAndStartGameCoroutine != null;
        public bool IsNextHomeworkLastForPhase => HomeworksNeededToFinishGame <= HomeworksHanded + 1;
        public float GameProgression => HomeworksHanded / (float)HomeworksNeededToFinishGame; // Between 0 and 1

        public bool IsRunning => currentState == GameState.Running;

        protected override void Awake()
        {
            base.Awake();

            settings = SettingsSystem.GameSettings;
            networkedData = GetComponent<NetworkedGameData>();
        }
        
        private void Start()
        {
            networkedData.OnGameStateChanged += HandleGameStateChanged;

            LevelSystem.Instance.OnGameLoad += OnGameLoaded;
        }

        protected override void OnDestroy()
        {
            networkedData.OnGameStateChanged -= HandleGameStateChanged;
            
            if (LevelSystem.HasInstance)
                LevelSystem.Instance.OnGameLoad -= OnGameLoaded;

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

        public void EndGame()
        {
            if (currentState != GameState.Running)
                return;
            
            networkedData.GameIsEnded = true;
        }

        public void Reset()
        {
            networkedData.Reset();
            
            OnReset?.Invoke();
        }
        
        public void IncrementHomeworksGivenForPhase()
        {
            ++networkedData.PhaseTotalHomework;
            if (HomeworksHanded == HomeworksNeededToFinishGame)
            {
                EndGame();
            }
        }
    }
}