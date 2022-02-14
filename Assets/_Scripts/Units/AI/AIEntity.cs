using System;
using Fusion;
using UnityEngine;
using UnityEngine.AI;

namespace Units.AI
{
    // ReSharper disable once InconsistentNaming Reason: AI should be uppercase
    [RequireComponent(typeof(NavMeshAgent))]
    public class AIEntity : NetworkBehaviour
    {
        private NavMeshAgent agent;
        private AIBrain brain = null;

        public NavMeshAgent Agent => agent;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
        }

        public void AddBrain(GameObject brainPrefab)
        {
            if (brain)
            {
                Destroy(brain);
                brain = null;
            }

            var brainGameObject = Instantiate(brainPrefab, transform);
            brainGameObject.name = brainPrefab.name;

            brain = brainGameObject.GetComponent<AIBrain>();
            Debug.Assert(brain, $"Calling {nameof(AddBrain)} with a prefab that doesn't have a script {nameof(AIBrain)}");
            
            brain.AssignEntity(this);
        }
    }
}