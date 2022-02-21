using Fusion;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;

namespace Units.AI
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Inventory))]
    [ValidateInput(nameof(ValidateIfHasTag), "An AIEntity component must be placed on a collider that has the 'AI' tag.")]
    public class AIEntity : NetworkBehaviour
    {
        public const string TAG = "AI";

        [SerializeField, Tooltip("Only use if this AI cannot be spawned by the AI Manager")] 
        private GameObject brainToAddOnSpawned;

        private NavMeshAgent agent;
        private AIBrain brain;

        public NavMeshAgent Agent => agent;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
        }

        public override void Spawned()
        {
            if (brainToAddOnSpawned && Object.HasStateAuthority)
            {
                AddBrain(brainToAddOnSpawned);
            }
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

        private bool ValidateIfHasTag()
        {
            return gameObject.CompareTag(TAG);
        }
    }
}