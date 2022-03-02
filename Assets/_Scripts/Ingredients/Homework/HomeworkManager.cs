using System;
using System.Collections.Generic;
using Fusion;
using Scriptables;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Systems;
using Systems.Network;
using UnityEngine;
using Utilities.Extensions;
using Utilities.Singleton;

namespace Ingredients.Homework
{
    public class HomeworkManager : Singleton<HomeworkManager>
    {
        [SerializeField, Required] private NetworkObject homeworkPrefab;
        
        private readonly Dictionary<int, Homework> homeworks = new Dictionary<int, Homework>();
        private HomeworkSpawnPoint[] spawnPoints = Array.Empty<HomeworkSpawnPoint>();

        private HomeworkSettings settings;

        public void RegisterHomework(Homework homework)
        {
            homeworks.Add(homework.HomeworkId, homework);
        }

        public void UnregisterHomework(Homework homework)
        {
            homeworks.Remove(homework.HomeworkId);
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

            settings = SettingsSystem.Instance.HomeworkSettings;
        }

        private float spawnTimer = 0f;
        private void Update()
        {
            if (!NetworkSystem.Instance.IsHost || spawnPoints.IsNullOrEmpty())
                return;
            
            foreach (var homework in homeworks.Values)
            {
                if (homework.transform.position.y < -10f)
                {
                    NetworkSystem.Instance.Despawn(homework.Object); // TODO Return to pool
                }
            }
            
            spawnTimer += Time.deltaTime;
            if (spawnTimer > 5f)
            {
                spawnTimer = 0f;
                var randomSpawnPoint = spawnPoints.RandomElement();
                
                var spawnerTransform = randomSpawnPoint.transform;
                NetworkSystem.Instance.Spawn(homeworkPrefab, spawnerTransform.position, spawnerTransform.rotation, null, ManageHomeworkBeforeSpawned);
            }
        }

        private void ManageHomeworkBeforeSpawned(NetworkRunner runner, NetworkObject homeworkObject)
        {
            var homework = homeworkObject.GetComponent<Homework>();
            Debug.Assert(homework);
            homework.Free();
        }
    }
}