using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Systems.Network;
using Fusion;
using Managers.Game;
using Managers.Hallway;
using Systems.Level;
using Units.AI.Senses;
using Units.Customization;
using UnityEngine;
using Utilities.Singleton;

namespace Units.AI
{
    public class AIManager : Singleton<AIManager>
    {
        private AIEntity teacher;
        private readonly List<AIEntity> students = new List<AIEntity>();
        private readonly List<AIEntity> janitors = new List<AIEntity>();

        private readonly List<AIEntity> aisToSpawn = new List<AIEntity>();

        public AIEntity Teacher => teacher;
        public IEnumerable<AIEntity> Janitors => janitors;
        public IEnumerable<AIEntity> Students => students;

        public IEnumerable<AIEntity> AllAIs => Teacher ? students.Concat(janitors).Concat(new[] { teacher }) : students.Concat(janitors);

        public void RegisterTeacher(AIEntity teacherAI)
        {
            Debug.Assert(!teacher,
                "Trying to assign a teacher when there is already another teacher (there can only be one teacher)");
            teacher = teacherAI;

            UpdateAISpawned(teacherAI);
        }

        public void UnregisterTeacher(AIEntity teacherAI)
        {
            if (teacher != teacherAI)
            {
                Debug.LogWarning("Trying to unregister a teacher that was not the current teacher");
                return;
            }
            teacher = null;
        }

        public void RegisterStudent(AIEntity student)
        {
            students.Add(student);

            student.GetComponent<AICustomization>().LocalRandomize();

            UpdateAISpawned(student);
        }

        public void UnregisterStudent(AIEntity student)
        {
            students.Remove(student);
        }

        public void RegisterJanitor(AIEntity janitor)
        {
            janitors.Add(janitor);

            UpdateAISpawned(janitor);
        }

        public void UnregisterJanitor(AIEntity janitor)
        {
            janitors.Remove(janitor);
        }

        private void UpdateAISpawned(AIEntity aiEntity)
        {
            aisToSpawn.Remove(aiEntity);

            if (!GameManager.HasInstance)
                return;

            if (GameManager.Instance.IsSpawning && aisToSpawn.Count == 0)
            {
                GameManager.Instance.UnlockSpawn(this);
            }
        }

        private void Start()
        {
            if (GameManager.HasInstance)
            {
                GameManager.Instance.OnBeginSpawn += SpawnAIsFromSpawnLocations;
                GameManager.Instance.OnBeginDespawn += DespawnAIs;
            }
            else
            {
                StartCoroutine(SpawnAIsWhenConnectedRoutine());
                LevelSystem.Instance.OnBeforeUnload += DespawnAIs;
            }
        }

        private IEnumerator SpawnAIsWhenConnectedRoutine()
        {
            yield return new WaitUntil(() => NetworkSystem.Instance.IsConnected);
            SpawnAIsFromSpawnLocations();
        }

        protected override void OnDestroy()
        {
            if (GameManager.HasInstance)
            {
                GameManager.Instance.OnBeginSpawn -= SpawnAIsFromSpawnLocations;
                GameManager.Instance.OnBeginDespawn -= DespawnAIs;
            }

            if (LevelSystem.HasInstance)
            {
                LevelSystem.Instance.OnBeforeUnload -= DespawnAIs;
            }
            
            StopAllCoroutines();
            base.OnDestroy();
        }

        private void SpawnAIsFromSpawnLocations()
        {
            if (GameManager.HasInstance)
                GameManager.Instance.LockSpawn(this);

            foreach (var spawnLocation in FindObjectsOfType<AISpawnLocation>())
            {
                SpawnAI(spawnLocation);
            }

            if (GameManager.HasInstance)
                GameManager.Instance.UnlockSpawn(this);
        }

        private void DespawnAIs()
        {
            if (!NetworkSystem.HasInstance)
                return;

            if (teacher)
                NetworkSystem.Instance.Despawn(teacher.Object);


            foreach (var janitor in janitors.ToList())
            {
                NetworkSystem.Instance.Despawn(janitor.Object);
            }

            foreach (var student in students.ToList())
            {
                NetworkSystem.Instance.Despawn(student.Object);
            }
        }

        private void SpawnAI(AISpawnLocation spawnLocation)
        {
            var spawnLocationTransform = spawnLocation.transform;
            NetworkSystem.Instance.Spawn(
                spawnLocation.AIEntityPrefab,
                spawnLocationTransform.position,
                spawnLocationTransform.rotation,
                null,
                (runner, aiObject) => SetupAIEntityBeforeSpawn(aiObject, spawnLocation.AssignedHallway, spawnLocation.AIBrainPrefab));
        }

        private void SetupAIEntityBeforeSpawn(NetworkObject aiObject, HallwayColor assignedHallway, GameObject brainPrefab)
        {
            var entity = aiObject.GetComponent<AIEntity>();
            Debug.Assert(entity, $"An AI must have a {nameof(AIEntity)} attached");
            
            entity.AddBrain(brainPrefab);
            entity.AssignHallway(assignedHallway);

            var homeworkHandingStation = aiObject.GetComponentInChildren<HomeworkHandingStation>();
            var janitorVision = aiObject.GetComponent<JanitorVision>();

            if (homeworkHandingStation) entity.MarkAsTeacher();
            else if (janitorVision) entity.MarkAsJanitor();
            else entity.MarkAsStudent();
            
            aisToSpawn.Add(entity);
        }
    }
}