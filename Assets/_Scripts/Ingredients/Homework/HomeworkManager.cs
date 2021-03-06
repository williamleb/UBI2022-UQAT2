using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using JetBrains.Annotations;
using Managers.Game;
using Sirenix.Utilities;
using Systems;
using Systems.Level;
using Systems.Network;
using Systems.Settings;
using UnityEngine;
using Utilities;
using Utilities.Event;
using Utilities.Extensions;
using Utilities.Singleton;
using Random = UnityEngine.Random;

namespace Ingredients.Homework
{
    public class HomeworkManager : Singleton<HomeworkManager>
    {
        private const float SECURITY_NET_POSITION = 10f;

        public MemoryEvent<Homework> OnHomeworkRegistered;

        private readonly Dictionary<int, Homework> homeworks = new Dictionary<int, Homework>();
        private HomeworkSpawnPoint[] spawnPoints = Array.Empty<HomeworkSpawnPoint>();

        private readonly List<Homework> homeworksToSpawn = new List<Homework>();

        private readonly Dictionary<string, int> cooldowns = new Dictionary<string, int>();
        private readonly List<BurstWithType> allBurstToDo = new List<BurstWithType>();
        private readonly List<BurstWithType> activeBursts = new List<BurstWithType>();

        private HomeworkSettings settings;

        private Coroutine activateHomeworkCoroutine;
        private Coroutine verifyHomeworkStateCoroutine;

        public List<Homework> Homeworks => homeworks.Values.ToList();

        public void RegisterHomework(Homework homework)
        {
            homeworks.Add(homework.HomeworkId, homework);
            OnHomeworkRegistered.InvokeWithMemory(homework);
            UpdateHomeworkSpawned(homework);
        }

        public void UnregisterHomework(Homework homework)
        {
            homeworks.Remove(homework.HomeworkId);
            OnHomeworkRegistered.RemoveFromMemory(homework);
        }

        public Homework GetHomework(int homeworkId)
        {
            if (!homeworks.TryGetValue(homeworkId, out var homework))
                return null;

            return homework;
        }

        public void RemoveHomework(int homeworkId)
        {
            if (!homeworks.TryGetValue(homeworkId, out var homework))
                return;
            
            homework.Free();
        }

        public int GetScoreValueForHomeworkType(string type)
        {
            foreach (var definition in settings.HomeworkDefinitions)
            {
                if (definition.Type.Equals(type))
                    return definition.Points;
            }

            Debug.Assert(false);
            return -1;
        }

        public HomeworkDefinition GetHomeworkDefinitionFromScoreValue(int scoreValue)
        {
            foreach (var definition in settings.HomeworkDefinitions)
            {
                if (definition.Points == scoreValue)
                    return definition;
            }

            return null;
        }

        private void Start()
        {
            settings = SettingsSystem.HomeworkSettings;

            if (GameManager.HasInstance)
            {
                GameManager.Instance.OnBeginSpawn += SpawnHomeworks;
                GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
                GameManager.Instance.OnBeginDespawn += DespawnHomeworks;
            }
            else
            {
                StartCoroutine(SpawnHomeworksAndInitializeWhenConnectedRoutine());
                LevelSystem.Instance.OnBeforeUnload += DespawnHomeworks;
            }
        }

        private IEnumerator SpawnHomeworksAndInitializeWhenConnectedRoutine()
        {
            yield return new WaitUntil(() => NetworkSystem.Instance.IsConnected);
            SpawnHomeworks();
            InitializeManager();
        }
        
        protected override void OnDestroy()
        {
            if (GameManager.HasInstance)
            {
                GameManager.Instance.OnBeginSpawn -= SpawnHomeworks;
                GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
                GameManager.Instance.OnBeginDespawn -= DespawnHomeworks;
            }

            if (LevelSystem.HasInstance)
            {
                LevelSystem.Instance.OnBeforeUnload -= DespawnHomeworks;
            }
            
            StopAllCoroutines();
            base.OnDestroy();
        }
        
        private void UpdateHomeworkSpawned(Homework homework)
        {
            homeworksToSpawn.Remove(homework);

            if (!GameManager.HasInstance)
                return;

            if (GameManager.Instance.IsSpawning && homeworksToSpawn.Count == 0)
            {
                GameManager.Instance.UnlockSpawn(this);
            }
        }

        private void OnGameStateChanged(GameState newGameState)
        {
            if (newGameState == GameState.Running)
            {
                InitializeManager();
            }else if (newGameState == GameState.Overtime)
            {
                //TODO do we need to do something?
            }
            else
            {
                StopHomeworkRoutines();
            }
        }

        private void InitializeManager()
        {
            InitializeSpawnPoints();
            InitializeCooldowns();
            InitializeBursts();
            StartHomeworkRoutines();
        }
        
        private void InitializeSpawnPoints()
        {
            spawnPoints = FindObjectsOfType<HomeworkSpawnPoint>();
            if (spawnPoints.IsNullOrEmpty())
            {
                Debug.LogWarning("No spawn points for homeworks found in the scene. We won't be able to spawn homeworks.");
            }
        }

        private void StartHomeworkRoutines()
        {
            if (activateHomeworkCoroutine != null)
                return;
            
            activateHomeworkCoroutine = StartCoroutine(ActivateHomeworkRoutine());
            verifyHomeworkStateCoroutine = StartCoroutine(VerifyHomeworkStateRoutine());
        }

        private void StopHomeworkRoutines()
        {
            if (activateHomeworkCoroutine == null)
                return;
            
            StopCoroutine(activateHomeworkCoroutine);
            activateHomeworkCoroutine = null;
            
            StopCoroutine(verifyHomeworkStateCoroutine);
            verifyHomeworkStateCoroutine = null;
        }

        private IEnumerator ActivateHomeworkRoutine()
        {
            while (true)
            {
                var secondsToWaitBeforeSpawn = Random.Range(settings.MinSecondsBeforeHomeworkSpawn, settings.MaxSecondsBeforeHomeworkSpawn);
                yield return Helpers.GetWait(secondsToWaitBeforeSpawn);
                yield return new WaitUntil(DoesNotHaveMaximumAmountOfHomeworksActivated);
                
                ActivateHomework();
            }
            // ReSharper disable once IteratorNeverReturns Reason: this loops until the coroutine stops from external sources
        }

        private IEnumerator VerifyHomeworkStateRoutine()
        {
            while (true)
            {
                yield return Helpers.GetWait(1f);
                
                FreeAllHomeworkPastSecurityNet();
                ActivateHomeworkIfEveryHomeworkIsFree();
            }
            // ReSharper disable once IteratorNeverReturns Reason: this loops until the coroutine stops from external sources
        }

        private void FreeAllHomeworkPastSecurityNet()
        {
            foreach (var homework in homeworks.Values)
            {
                if (homework && homework.transform.position.y < -SECURITY_NET_POSITION)
                {
                    homework.Free();
                }
            }
        }

        private void ActivateHomeworkIfEveryHomeworkIsFree()
        {
            if (IsEveryHomeworkFree())
            {
                ActivateHomework();
            }
        }

        private void SpawnHomeworks()
        {
            if (GameManager.HasInstance)
                GameManager.Instance.LockSpawn(this);

            foreach (var homeworkDefinition in settings.HomeworkDefinitions)
            {
                for (var i = 0; i < Math.Min(settings.MaxNumberOfHomeworksInPlay, homeworkDefinition.MaxAmountAtTheSameTime); ++i)
                {
                    NetworkSystem.Instance.Spawn(homeworkDefinition.Prefab, Vector3.down * 1000f, Quaternion.identity, null, (runner, o) => ManageHomeworkBeforeSpawned(o, homeworkDefinition.Type));
                }
            }
        }

        private void DespawnHomeworks()
        {
            if (!NetworkSystem.HasInstance)
                return;
            
            foreach (var homework in homeworks.ToList())
            {
                NetworkSystem.Instance.Despawn(homework.Value.Object);
            }
        }

        private void ActivateHomework()
        {
            var homeworkToActivate = ChooseHomeworkToSpawn();
            if (!homeworkToActivate)
                return;

            var spawnPoint = ChooseSpawnPoint();
            if (!spawnPoint)
                return;
            
            UpdateCooldown(homeworkToActivate.Type);
            homeworkToActivate.Activate(spawnPoint.transform);
        }

        private bool DoesNotHaveMaximumAmountOfHomeworksActivated()
        {
            return homeworks.Values.Count(homework => !homework.IsFree) < Mathf.CeilToInt(PlayerSystem.Instance.NumberOfPlayers / 2f);
        }

        private bool IsEveryHomeworkFree()
        {
            return homeworks.Values.Count(homework => homework.IsFree) == homeworks.Count;
        }

        private bool AnyLeftToSpawnForType(HomeworkDefinition homeworkDefinition)
        {
            return homeworks.Values.Count(homework => !homework.IsFree && homework.Type.Equals(homeworkDefinition.Type)) < homeworkDefinition.MaxAmountAtTheSameTime;
        }

        private bool HasAnyHomeworkNear(Vector3 homeworkSpawnerPosition)
        {
            return homeworks.Values.Any(homework => Vector3.SqrMagnitude(homeworkSpawnerPosition - homework.transform.position) < 0.5f);
        }

        [CanBeNull] 
        private HomeworkSpawnPoint ChooseSpawnPoint()
        {
            var validSpawnPoints = spawnPoints.Where(spawnPoint => !HasAnyHomeworkNear(spawnPoint.transform.position)).ToArray();

            return validSpawnPoints.Any() ? validSpawnPoints.WeightedRandomElement() : null;
        }

        private void ManageHomeworkBeforeSpawned(NetworkObject homeworkObject, string type)
        {
            var homework = homeworkObject.GetComponent<Homework>();
            Debug.Assert(homework);
            homework.AssignType(type);
            homework.Free();
            homeworksToSpawn.Add(homework);
        }

        private HomeworkDefinition GetHomeworkDefinitionFromType(string type)
        {
            foreach (var definition in settings.HomeworkDefinitions)
            {
                if (definition.Type.Equals(type))
                    return definition;
            }
            
            Debug.Assert(false);
            return null;
        }

        private Homework ChooseHomeworkToSpawn()
        {
            var homeworkFromBurst = GetHomeworkFromBurst();
            if (homeworkFromBurst)
                return homeworkFromBurst;

            var chosenHomeworkDefinition = 
                settings.HomeworkDefinitions
                    .Where(AnyLeftToSpawnForType)
                    .Where(IsCooldownFinished)
                    .WeightedRandomElement();
            
            return GetNextFreeHomeworkOfType(chosenHomeworkDefinition.Type);
        }
        
        [CanBeNull]
        private Homework GetNextFreeHomeworkOfType(string type)
        {
            foreach (var homework in homeworks.Values)
            {
                if (homework.IsFree && homework.Type.Equals(type))
                {
                    return homework;
                }
            }
            
            return null;
        }
        
        private void InitializeCooldowns()
        {
            cooldowns.Clear();
            foreach (var homeworkDefinition in settings.HomeworkDefinitions)
            {
                cooldowns.Add(homeworkDefinition.Type, 0);
            }
        }

        private void UpdateCooldown(string typeThatJustSpawned)
        {
            foreach (var type in cooldowns.Keys.ToList())
            {
                cooldowns[type]--;
            }
            
            if (cooldowns.ContainsKey(typeThatJustSpawned))
            {
                cooldowns[typeThatJustSpawned] = GetHomeworkDefinitionFromType(typeThatJustSpawned).Cooldown;
            }
        }

        private bool IsCooldownFinished(HomeworkDefinition homeworkDefinition)
        {
            if (!cooldowns.ContainsKey(homeworkDefinition.Type))
                return true;

            return cooldowns[homeworkDefinition.Type] <= 0;
        }

        private readonly List<BurstWithType> burstsToRemove = new List<BurstWithType>();
        private readonly List<BurstWithType> burstsThatCanBeDone = new List<BurstWithType>();
        [CanBeNull]
        private Homework GetHomeworkFromBurst()
        {
            UpdateActiveBursts();
            
            burstsThatCanBeDone.Clear();
            burstsToRemove.Clear();

            foreach (var burst in activeBursts)
            {
                var homeworkDefinition = GetHomeworkDefinitionFromType(burst.HomeworkType);
                if (!AnyLeftToSpawnForType(homeworkDefinition))
                {
                    burstsToRemove.Add(burst);
                    continue;
                }

                var shouldActivateHomework = Random.Range(0f, 1f) <= burst.Burst.Probability;
                if (!shouldActivateHomework)
                {
                    if (!burst.Burst.Retry)
                    {
                        burstsToRemove.Add(burst);
                    }
                    continue;
                }
                
                burstsThatCanBeDone.Add(burst);
            }

            foreach (var burstToRemove in burstsToRemove)
            {
                activeBursts.Remove(burstToRemove);
            }

            if (!burstsThatCanBeDone.Any())
                return null;

            var burstDefinitionToDo = burstsThatCanBeDone.Select(burst => GetHomeworkDefinitionFromType(burst.HomeworkType)).WeightedRandomElement();
            activeBursts.RemoveAt(activeBursts.FindIndex(burst => burst.HomeworkType == burstDefinitionToDo.Type));
            return GetNextFreeHomeworkOfType(burstDefinitionToDo.Type);
        }
        
        private void UpdateActiveBursts()
        {
            burstsToRemove.Clear();
            foreach (var burstToDo in allBurstToDo)
            {
                if (GameManager.HasInstance)
                {
                    if (burstToDo.Burst.AtProgress <= GameManager.Instance.GameProgression)
                    {
                        burstsToRemove.Add(burstToDo);
                    }
                }
                else
                {
                    return;
                }
            }

            foreach (var burstToUpdate in burstsToRemove)
            {
                allBurstToDo.Remove(burstToUpdate);
                activeBursts.Add(burstToUpdate);
            }
        }
        
        private void InitializeBursts()
        {
            allBurstToDo.Clear();
            foreach (var homeworkDefinition in settings.HomeworkDefinitions)
            {
                foreach (var burst in homeworkDefinition.Bursts)
                {
                    var burstToAdd = new BurstWithType()
                    {
                        HomeworkType = homeworkDefinition.Type,
                        Burst = burst
                    };
                    allBurstToDo.Add(burstToAdd);
                }
            }
        }

        private struct BurstWithType
        {
            public string HomeworkType;
            public HomeworkDefinition.Burst Burst;
        }
    }
}