using Fusion;
using UnityEngine;
using UnityEngine.AI;
using Utilities.Extensions;
using Utilities.Unity;

namespace Units.AI
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Inventory))]
    [RequireComponent(typeof(AIInteracter))]
    public class AIEntity : NetworkBehaviour
    {
        [SerializeField, Tooltip("Only use if this AI cannot be spawned by the AI Manager")] 
        private GameObject brainToAddOnSpawned;

        private NavMeshAgent agent;
        private Inventory inventory;
        private AIInteracter interacter;
        private AIBrain brain;
        
        [Networked] public bool IsTeacher { get; private set; }

        public NavMeshAgent Agent => agent;
        public Inventory Inventory => inventory;
        public AIInteracter Interacter => interacter;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            inventory = GetComponent<Inventory>();
            interacter = GetComponent<AIInteracter>();
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
            brainGameObject.transform.position = Vector3.zero;
            brainGameObject.name = brainPrefab.name;

            brain = brainGameObject.GetComponent<AIBrain>();
            Debug.Assert(brain, $"Calling {nameof(AddBrain)} with a prefab that doesn't have a script {nameof(AIBrain)}");
            
            brain.AssignEntity(this);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!Application.isPlaying)
                UnityEditor.EditorApplication.delayCall += AssignTagAndLayer;
        }

        private void AssignTagAndLayer()
        {
            if (this == null)
                return;
            
            var thisGameObject = gameObject;
            
            if (!thisGameObject.AssignTagIfDoesNotHaveIt(Tags.AI))
                Debug.LogWarning($"Player {thisGameObject.name} should have the tag {Tags.AI}. Instead, it has {thisGameObject.tag}");
            
            if (!thisGameObject.AssignLayerIfDoesNotHaveIt(Layers.GAMEPLAY))
                Debug.LogWarning($"Player {thisGameObject.name} should have the layer {Layers.GAMEPLAY} ({Layers.NAME_GAMEPLAY}). Instead, it has {thisGameObject.layer}");
        }
#endif
    }
}