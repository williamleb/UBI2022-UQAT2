using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using JetBrains.Annotations;
using Managers.Game;
using Sirenix.Utilities;
using Systems.Network;
using Systems.Settings;
using UnityEngine;
using Utilities.Extensions;
using Utilities.Singleton;
using Random = UnityEngine.Random;

namespace Ingredients.Homework
{
    public class HomeworkManager : Singleton<HomeworkManager>
    {
        private const float SECURITY_NET_POSITION = 10f;
        
        private readonly Dictionary<int, Homework> homeworks = new Dictionary<int, Homework>();
        private HomeworkSpawnPoint[] spawnPoints = Array.Empty<HomeworkSpawnPoint>();

        private readonly List<Homework> homeworksToSpawn = new List<Homework>();

        private readonly Dictionary<string, int> cooldowns = new Dictionary<string, int>();
        private readonly List<BurstWithType> allBurstToDo = new List<BurstWithType>();
        private readonly List<BurstWithType> activeBursts = new List<BurstWithType>();

        private HomeworkSettings settings;

        private Coroutine activateHomeworkCoroutine;
        private Coroutine verifyHomeworkStateCoroutine;

        public void RegisterHomework(Homework homework)
        {
            homeworks.Add(homework.HomeworkId, homework);
            UpdateHomeworkSpawned(homework);
        }

        public void UnregisterHomework(Homework homework)
        {
            homeworks.Remove(homework.HomeworkId);
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

        private void Start()
        {
            spawnPoints = FindObjectsOfType<HomeworkSpawnPoint>();
            if (spawnPoints.IsNullOrEmpty())
            {
                Debug.LogWarning("No spawn points for homeworks found in the scene. We won't be able to spawn homeworks.");
            }

            settings = SettingsSystem.HomeworkSettings;

            if (GameManager.HasInstance)
            {
                GameManager.Instance.OnBeginSpawn += SpawnHomeworks;
                GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
            }
        }
        
        protected override void OnDestroy()
        {
            if (GameManager.HasInstance)
            {
                GameManager.Instance.OnBeginSpawn -= SpawnHomeworks;
                GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
            }
            
            base.OnDestroy();
        }

        private void OnGameStateChanged(GameState newGameState)
        {
            if (newGameState == GameState.Running)
            {
                InitializeCooldowns();
                InitializeBursts();
                StartHomeworkRoutines();
            }
            else
            {
                StopHomeworkRoutines();
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
                yield return new WaitForSeconds(secondsToWaitBeforeSpawn);
                yield return new WaitUntil(DoesNotHaveMaximumAmountOfHomeworksActivated);
                
                ActivateHomework();
            }
            // ReSharper disable once IteratorNeverReturns Reason: this loops until the coroutine stops from external sources
        }

        private IEnumerator VerifyHomeworkStateRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(1f);
                
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
            GameManager.Instance.LockSpawn(this);

            foreach (var homeworkDefinition in settings.HomeworkDefinitions)
            {
                for (var i = 0; i < Math.Min(settings.MaxNumberOfHomeworksInPlay, homeworkDefinition.MaxAmountAtTheSameTime); ++i)
                {
                    NetworkSystem.Instance.Spawn(homeworkDefinition.Prefab, Vector3.down * 1000f, Quaternion.identity, null, (runner, o) =>  ManageHomeworkBeforeSpawned(runner, o, homeworkDefinition.Type));
                }
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
            return homeworks.Values.Count(homework => !homework.IsFree) < settings.MaxNumberOfHomeworksInPlay;
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

        private void ManageHomeworkBeforeSpawned(NetworkRunner runner, NetworkObject homeworkObject, string type)
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

        private bool IsCooldownFinished(HomeworkDefinition homrworkDefinition)
        {
            if (!cooldowns.ContainsKey(homrworkDefinition.Type))
                return true;

            return cooldowns[homrworkDefinition.Type] <= 0;
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
                if (burstToDo.Burst.AtProgress <= GameManager.Instance.GameProgression)
                {
                    burstsToRemove.Add(burstToDo);
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