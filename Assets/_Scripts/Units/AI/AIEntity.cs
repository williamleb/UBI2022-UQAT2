using System.Collections;
using Fusion;
using Interfaces;
using Managers.Hallway;
using Systems.Settings;
using Units.AI.Senses;
using UnityEngine;
using UnityEngine.AI;
using Utilities.Extensions;
using Utilities.Unity;

namespace Units.AI
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Inventory))]
    [RequireComponent(typeof(AIInteracter))]
    public class AIEntity : NetworkBehaviour, IVelocityObject
    {
        private static readonly int walking = Animator.StringToHash("IsWalking");
        
        private enum AIType : byte
        {
            Student = 0,
            Teacher,
            Janitor
        }
        
        [SerializeField, Tooltip("Only use if this AI cannot be spawned by the AI Manager")] 
        private GameObject brainToAddOnSpawned;

        [SerializeField] 
        private NetworkObject aiCollider;

        private NavMeshAgent agent;
        private Inventory inventory;
        private AIInteracter interacter;
        private Animator animator;
        private NetworkMecanimAnimator networkAnimator;
        private PlayerBadBehaviorDetection playerBadBehaviorDetection;
        private HomeworkHandingStation homeworkHandingStation;
        private AITaskSensor taskSensor;
        private AIBrain brain;

        private AISettings settings;
        private Transform aiColliderTransform;
        private Coroutine hitCoroutine = null;

        [Networked, Capacity(8)] private AIType Type { get; set; }
        private bool IsTeacher => Type == AIType.Teacher;
        private bool IsJanitor => Type == AIType.Janitor;
        
        [Networked, Capacity(8)] public HallwayColor AssignedHallway{ get; private set; }

        public NavMeshAgent Agent => agent;
        public Inventory Inventory => inventory;
        public AIInteracter Interacter => interacter;
        public Animator Animator => animator;
        public NetworkMecanimAnimator NetworkAnimator => networkAnimator;
        public PlayerBadBehaviorDetection PlayerBadBehaviorDetection => playerBadBehaviorDetection;
        public HomeworkHandingStation HomeworkHandingStation => homeworkHandingStation;
        public AITaskSensor TaskSensor => taskSensor;
        public Vector3 Velocity => agent.velocity;

        public bool IsHit => hitCoroutine != null;

        public float BaseSpeed => IsTeacher ? settings.BaseTeacherSpeed : IsJanitor ? settings.BaseJanitorSpeed : settings.BaseStudentSpeed;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            inventory = GetComponent<Inventory>();
            interacter = GetComponent<AIInteracter>();
            animator = GetComponent<Animator>();
            networkAnimator = GetComponent<NetworkMecanimAnimator>();
            playerBadBehaviorDetection = GetComponent<PlayerBadBehaviorDetection>();
            homeworkHandingStation = GetComponentInChildren<HomeworkHandingStation>();
            taskSensor = GetComponent<AITaskSensor>();
            settings = SettingsSystem.AISettings;

            inventory.AssignVelocityObject(this);
        }

        // Those three methods should only be called before the AI entity is spawned
        public void MarkAsTeacher() => Type = AIType.Teacher;
        public void MarkAsStudent() => Type = AIType.Student;
        public void MarkAsJanitor() => Type = AIType.Janitor;

        public void AssignHallway(HallwayColor hallwayColor) => AssignedHallway = hallwayColor;

        public override void Spawned()
        {
            if (Object.HasStateAuthority)
            {
                if (brainToAddOnSpawned)
                    AddBrain(brainToAddOnSpawned);
                
                SpawnCollider();
            }

            RegisterToManager();
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            UnspawnCollider();
            UnregisterToManager();
        }

        private void SpawnCollider()
        {
            if (!aiCollider) 
                return;
            
            var thisTransform = transform;
            var aiColliderObject = Runner.Spawn(aiCollider, thisTransform.position, thisTransform.rotation);

            aiColliderTransform = aiColliderObject.transform;
        }

        private void UnspawnCollider()
        {
            if (!aiColliderTransform)
                return;
            
            Runner.Despawn(aiColliderTransform.GetComponent<NetworkObject>());
        }

        public override void FixedUpdateNetwork()
        {
            UpdateCollider();
            UpdateWalkingAnimation();
        }

        private void UpdateCollider()
        {
            if (!aiColliderTransform)
                return;

            var thisTransform = transform;
            aiColliderTransform.position = thisTransform.position;
            aiColliderTransform.rotation = thisTransform.rotation;
        }

        private void UpdateWalkingAnimation()
        {
            if (!Object.HasStateAuthority)
                return;

            if (!animator)
                return;

            // We will probably want to send the speed directly to the animator in the future and do a blend space
            var isWalking = agent.velocity.sqrMagnitude > 0.3f;
            animator.SetBool(walking, isWalking);
        }

        private void RegisterToManager()
        {
            if (!AIManager.HasInstance)
                return;

            if (IsTeacher)
                AIManager.Instance.RegisterTeacher(this);
            else if (IsJanitor)
                AIManager.Instance.RegisterJanitor(this);
            else
                AIManager.Instance.RegisterStudent(this);
        }

        private void UnregisterToManager()
        {
            if (!AIManager.HasInstance)
                return;

            if (IsTeacher)
                AIManager.Instance.UnregisterTeacher(this);
            else if (IsJanitor)
                AIManager.Instance.UnregisterJanitor(this);
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
            brainGameObject.transform.position = transform.position;
            brainGameObject.name = brainPrefab.name;

            brain = brainGameObject.GetComponent<AIBrain>();
            Debug.Assert(brain, $"Calling {nameof(AddBrain)} with a prefab that doesn't have a script {nameof(AIBrain)}");
            
            brain.AssignEntity(this);
        }

        public void Hit()
        {
            if (hitCoroutine != null)
            {
                StopHitRoutine();
            }
            
            hitCoroutine = StartCoroutine(HitRoutine());
        }

        private IEnumerator HitRoutine()
        {
            var secondsToWait = settings.SecondsDownAfterBeingHit;
            yield return new WaitForSeconds(secondsToWait);
            hitCoroutine = null;
        }
        
        private void StopHitRoutine()
        {
            if (hitCoroutine == null)
                return;
            
            StopCoroutine(hitCoroutine);
            hitCoroutine = null;
        }

        private void OnDisable()
        {
            StopHitRoutine();
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