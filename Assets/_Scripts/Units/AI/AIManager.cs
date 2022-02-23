using System.Collections.Generic;
using Systems.Network;
using Fusion;
using UnityEngine;
using Utilities.Singleton;

namespace Units.AI
{
    public class AIManager : Singleton<AIManager>
    {
        [SerializeField] private AIEntity entityPrefab;
        [SerializeField] private GameObject brainPrefab;

        private AIEntity teacher = null;
        private List<AIEntity> students = new List<AIEntity>();

        public void RegisterTeacher(AIEntity teacherAI)
        {
            Debug.Assert(!teacher, "Trying to assign a teacher when there is already another teacher (there can only be one teacher)");
            teacher = teacherAI;
        }

        public void UnregisterTeacher(AIEntity teacherAI)
        {
            Debug.Assert(teacher == teacherAI, "Trying to unregister a teacher that was not the current teacher");
            teacher = null;
        }

        public void RegisterStudent(AIEntity student)
        {
            students.Add(student);
        }

        public void UnregisterStudent(AIEntity student)
        {
            students.Remove(student);
        }

        private void Start()
        {
            NetworkSystem.Instance.OnPlayerJoinedEvent += OnPlayerJoined;
        }
        
        protected override void OnDestroy()
        {
            if (NetworkSystem.HasInstance)
            {
                NetworkSystem.Instance.OnPlayerJoinedEvent -= OnPlayerJoined;
            }
            
            base.OnDestroy();
        }

        private void OnPlayerJoined(NetworkRunner runner, PlayerRef playerRef)
        {
            if (!NetworkSystem.Instance.IsHost) 
                return;
            
            
            var entity = runner.Spawn(entityPrefab, Vector3.zero, Quaternion.identity, playerRef,
                (networkRunner, aiObject) =>
                {
                    var entity = aiObject.GetComponent<AIEntity>();
                    Debug.Assert(entity, $"An AI must have a {nameof(AIEntity)} attached");

                    var homeworkHandingStation = aiObject.GetComponentInChildren<HomeworkHandingStation>();
                    if (homeworkHandingStation) entity.MarkAsTeacher();
                    else entity.MarkAsStudent();
                });
            entity.AddBrain(brainPrefab);
        }
    }
}