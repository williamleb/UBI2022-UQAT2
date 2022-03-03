using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using Systems.Network;
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
        public event Action OnReset; // Only called on host
        public event Action<GameState> OnGameStateChanged; 
        public event Action OnPhaseTotalHomeworkChanged
        {
            add => networkedData.OnPhaseTotalHomeworkChanged += value;
            remove => networkedData.OnPhaseTotalHomeworkChanged -= value;
        }
        
        [SerializeField] private int totalNumberOfHomeworkToFinishPhase = 10; // TODO Replace with current phase info
        
        private NetworkedGameData networkedData;
        private GameState currentState;

        private Coroutine spawnAndStartGameCoroutine = null;
        private HashSet<MonoBehaviour> spawnLocks = new HashSet<MonoBehaviour>();

        public int PhaseTotalHomework => networkedData.PhaseTotalHomework;
        public int NumberOfHomeworkToFinishPhase => totalNumberOfHomeworkToFinishPhase;
        public GameState CurrentState => currentState;
        public bool IsSpawning => spawnAndStartGameCoroutine != null;
        public bool IsNextHomeworkLastForPhase => NumberOfHomeworkToFinishPhase <= PhaseTotalHomework + 1;

        public bool IsRunning => currentState == GameState.Running;

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
            if (PhaseTotalHomework == NumberOfHomeworkToFinishPhase)
            {
                EndGame(); // TODO Switch phase if still has phases
            }
        }
    }
}