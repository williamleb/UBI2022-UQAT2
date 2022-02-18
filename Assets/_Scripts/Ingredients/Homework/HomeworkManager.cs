using System;
using System.Collections.Generic;
using Fusion;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Systems.Network;
using UnityEngine;
using Utilities.Extensions;
using Utilities.Singleton;

namespace Ingredients.Homework
{
    public class HomeworkManager : Singleton<HomeworkManager>
    {
        [SerializeField, Required] private NetworkObject homeworkPrefab;
        
        private Dictionary<int, Homework> homeworks = new Dictionary<int, Homework>();
        private HomeworkSpawnPoint[] spawnPoints = Array.Empty<HomeworkSpawnPoint>();

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

        private void Start()
        {
            spawnPoints = FindObjectsOfType<HomeworkSpawnPoint>();
            if (spawnPoints.IsNullOrEmpty())
            {
                Debug.LogWarning("No spawn points for homeworks found in the scene. We won't be able to spawn homeworks.");
            }
        }

        // The homework spawning logic will change in the future
        // We could also reuse homeworks instead of spawning/despawning them
        private float spawnTimer = 0f;
        private void Update()
        {
            if (!NetworkSystem.Instance.IsConnected || !NetworkSystem.Instance.IsHost || spawnPoints.IsNullOrEmpty())
                return;
            
            foreach (var homework in homeworks.Values)
            {
                if (homework.transform.position.y < 10f)
                {
                    NetworkSystem.Instance.Despawn(homework.Object);
                }
            }
            
            spawnTimer += Time.deltaTime;
            if (spawnTimer > 5f)
            {
                spawnTimer = 0f;
                var randomSpawnPoint = spawnPoints.RandomElement();
                NetworkSystem.Instance.Spawn(homeworkPrefab, randomSpawnPoint.transform.position);
            }
        }
    }
}