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
        
        [Networked] public bool IsTeacher { get; private set; }

        public NavMeshAgent Agent => agent;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
        }

        // Those two methods should only be called before the AI entity is spawned
        public void MarkAsTeacher() => IsTeacher = true;
        public void MarkAsStudent() => IsTeacher = false;

        public override void Spawned()
        {
            if (brainToAddOnSpawned && Object.HasStateAuthority)
            {
                AddBrain(brainToAddOnSpawned);
            }

            RegisterToManager();
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            UnregisterToManager();
        }

        private void RegisterToManager()
        {
            if (!AIManager.HasInstance)
                return;

            if (IsTeacher)
                AIManager.Instance.RegisterTeacher(this);
            else
                AIManager.Instance.RegisterStudent(this);
        }

        private void UnregisterToManager()
        {
            if (!AIManager.HasInstance)
                return;

            if (IsTeacher)
                AIManager.Instance.UnregisterTeacher(this);
            else
                AIManager.Instance.UnregisterStudent(this);
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