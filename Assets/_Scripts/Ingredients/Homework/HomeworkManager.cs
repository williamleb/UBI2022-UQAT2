﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using JetBrains.Annotations;
using Managers.Game;
using Sirenix.OdinInspector;
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
        
        [SerializeField, Required] private NetworkObject homeworkPrefab;
        
        private readonly Dictionary<int, Homework> homeworks = new Dictionary<int, Homework>();
        private HomeworkSpawnPoint[] spawnPoints = Array.Empty<HomeworkSpawnPoint>();
        
        private readonly List<Homework> homeworksToSpawn = new List<Homework>();

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
                StartHomeworkRoutines();
            else
                StopHomeworkRoutines();
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
                yield return new WaitUntil(IsAnyHomeworkFree);
                
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
            
            for (var i = 0; i < settings.MaxNumberOfHomeworksInPlay; ++i)
            {
                NetworkSystem.Instance.Spawn(homeworkPrefab, Vector3.down * 1000f, Quaternion.identity, null, ManageHomeworkBeforeSpawned);
            }
        }

        private void ActivateHomework()
        {
            var homeworkToActivate = GetNextFreeHomework();
            if (!homeworkToActivate)
                return;

            var spawnPoint = ChooseSpawnPoint();
            if (!spawnPoint)
                return;
            
            homeworkToActivate.Activate(spawnPoint.transform);
        }

        private bool IsAnyHomeworkFree()
        {
            return homeworks.Values.Any(homework => homework.IsFree);
        }

        private Homework GetNextFreeHomework()
        {
            foreach (var homework in homeworks.Values)
            {
                if (homework.IsFree)
                {
                    return homework;
                }
            }
            
            return null;
        }
        
        private bool IsEveryHomeworkFree()
        {
            return homeworks.Values.Count(homework => homework.IsFree) == homeworks.Count;
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

        private void ManageHomeworkBeforeSpawned(NetworkRunner runner, NetworkObject homeworkObject)
        {
            var homework = homeworkObject.GetComponent<Homework>();
            Debug.Assert(homework);
            homework.Free();
            homeworksToSpawn.Add(homework);
        }
    }
}