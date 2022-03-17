using System.Collections.Generic;
using Systems.Network;
using Fusion;
using Managers.Game;
using Managers.Hallway;
using Units.AI.Senses;
using UnityEngine;
using Utilities.Extensions;
using Utilities.Singleton;

namespace Units.AI
{
    public class AIManager : Singleton<AIManager>
    {
        private AIEntity teacher = null;
        private readonly List<AIEntity> students = new List<AIEntity>();
        private readonly List<AIEntity> janitors = new List<AIEntity>();

        private readonly List<AIEntity> aisToSpawn = new List<AIEntity>();

        public AIEntity Teacher => teacher;
        public IEnumerable<AIEntity> Janitors => janitors;
        public IEnumerable<AIEntity> Students => students;

        public void RegisterTeacher(AIEntity teacherAI)
        {
            Debug.Assert(!teacher, "Trying to assign a teacher when there is already another teacher (there can only be one teacher)");
            teacher = teacherAI;
            
            UpdateAISpawned(teacherAI);
        }

        public void UnregisterTeacher(AIEntity teacherAI)
        {
            Debug.Assert(teacher == teacherAI, "Trying to unregister a teacher that was not the current teacher");
            teacher = null;
        }

        public void RegisterStudent(AIEntity student)
        {
            students.Add(student);
            
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
                GameManager.Instance.OnBeginSpawn += SpawnAIsFromSpawnLocations;
        }
        
        protected override void OnDestroy()
        {
            if (GameManager.HasInstance)
                GameManager.Instance.OnBeginSpawn -= SpawnAIsFromSpawnLocations;
            
            base.OnDestroy();
        }

        private void SpawnAIsFromSpawnLocations()
        {
            GameManager.Instance.LockSpawn(this);

            foreach (var spawnLocation in FindObjectsOfType<AISpawnLocation>())
            {
                SpawnAI(spawnLocation);
            }
            
            GameManager.Instance.UnlockSpawn(this);
        }

        private void SpawnAI(AISpawnLocation spawnLocation)
        {
            var spawnLocationTransform = spawnLocation.transform;
            var entityGameObject = NetworkSystem.Instance.Spawn(
                spawnLocation.AIEntityPrefab, 
                spawnLocationTransform.position, 
                spawnLocationTransform.rotation, 
                null, 
                (runner, aiObject) => SetupAIEntityBeforeSpawn(aiObject, spawnLocation.AssignedHallway));
            
            var entity = entityGameObject.GetComponentInEntity<AIEntity>();
            Debug.Assert(entity);
            entity.AddBrain(spawnLocation.AIBrainPrefab);
            
            aisToSpawn.Add(entity);
        }

        private void SetupAIEntityBeforeSpawn(NetworkObject aiObject, HallwayColor assignedHallway)
        {
            var entity = aiObject.GetComponent<AIEntity>();
            Debug.Assert(entity, $"An AI must have a {nameof(AIEntity)} attached");
            
            entity.AssignHallway(assignedHallway);

            var homeworkHandingStation = aiObject.GetComponentInChildren<HomeworkHandingStation>();
            var janitorVision = aiObject.GetComponent<JanitorVision>();

            if (homeworkHandingStation) entity.MarkAsTeacher();
            else if (janitorVision) entity.MarkAsJanitor();
            else entity.MarkAsStudent();
        }
    }
}