using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using Interfaces;
using Managers.Hallway;
using Systems.Settings;
using Systems.Sound;
using Units.AI.Senses;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Utilities;
using Utilities.Extensions;
using Utilities.Unity;
using BodyPart = Units.Player.BodyPart;

namespace Units.AI
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Inventory))]
    [RequireComponent(typeof(AIInteracter))]
    [RequireComponent(typeof(AkGameObj))]
    public class AIEntity : NetworkBehaviour, IVelocityObject, IAudioObject
    {
        private static readonly int SpeedParam = Animator.StringToHash("Speed");
        private static readonly int IsHoldingHomeworkParam = Animator.StringToHash("IsHoldingHomework");
        private static readonly int GetUpBackDownParam = Animator.StringToHash("GetUpBackDown");
        private static readonly int GetUpFaceDownParam = Animator.StringToHash("GetUpFaceDown");

        public static event Action<AIEntity> OnAISpawned;
        public static event Action<AIEntity> OnAIDespawned;
        public event Action<GameObject> OnHit;

        private enum AIType : byte
        {
            Student = 0,
            Teacher,
            Janitor
        }

        [SerializeField, Tooltip("Only use if this AI cannot be spawned by the AI Manager")]
        private GameObject brainToAddOnSpawned;

        [SerializeField] private ParticleSystem alertParticleEffect;
        [SerializeField] private Transform ragdollTransform;

        private readonly List<BodyPart> ragdollColliders = new List<BodyPart>();
       
        private bool isRagdoll;

        private AkGameObj audioObject;
        private NavMeshAgent agent;
        private Inventory inventory;
        private AIInteracter interacter;
        private NetworkMecanimAnimator networkAnimator;
        private PlayerBadBehaviorDetection playerBadBehaviorDetection;
        private HomeworkHandingStation homeworkHandingStation;
        private AITaskSensor taskSensor;
        private AIBrain brain;

        private Collider aiCollider;

        private AISettings settings;
        private Coroutine hitCoroutine;

        [Networked, Capacity(8)] private AIType Type { get; set; }
        private bool IsTeacher => Type == AIType.Teacher;
        private bool IsJanitor => Type == AIType.Janitor;

        [Networked, Capacity(8)] public HallwayColor AssignedHallway { get; private set; }

        public AkGameObj AudioObject => audioObject;
        public NavMeshAgent Agent => agent;
        public Inventory Inventory => inventory;
        public AIInteracter Interacter => interacter;
        public Animator Animator => networkAnimator.Animator;
        public NetworkMecanimAnimator NetworkAnimator => networkAnimator;
        public PlayerBadBehaviorDetection PlayerBadBehaviorDetection => playerBadBehaviorDetection;
        public HomeworkHandingStation HomeworkHandingStation => homeworkHandingStation;
        public AITaskSensor TaskSensor => taskSensor;
        public Vector3 Velocity => agent.velocity;

        public bool IsHit => hitCoroutine != null;

        public bool IsImmune { get; set; }

        public float BaseSpeed => IsTeacher ? settings.BaseTeacherSpeed :
            IsJanitor ? settings.BaseJanitorSpeed : settings.BaseStudentSpeed;

        private float MaxSpeed => IsTeacher ? settings.BaseTeacherSpeed :
            IsJanitor ? settings.ChaseBadBehaviorSpeed : settings.BaseStudentSpeed;

        private void Awake()
        {
            audioObject = GetComponent<AkGameObj>();
            agent = GetComponent<NavMeshAgent>();
            inventory = GetComponent<Inventory>();
            interacter = GetComponent<AIInteracter>();
            networkAnimator = GetComponent<NetworkMecanimAnimator>();
            playerBadBehaviorDetection = GetComponent<PlayerBadBehaviorDetection>();
            homeworkHandingStation = GetComponentInChildren<HomeworkHandingStation>();
            taskSensor = GetComponent<AITaskSensor>();
            settings = SettingsSystem.AISettings;

            inventory.AssignVelocityObject(this);
            aiCollider = GetComponent<CapsuleCollider>();

            // We disable the agent until the AI is spawned so it doesn't teleport at the origin when it is spawned
            agent.enabled = false;
        }

        // Those three methods should only be called before the AI entity is spawned
        public void MarkAsTeacher() => Type = AIType.Teacher;
        public void MarkAsStudent() => Type = AIType.Student;
        public void MarkAsJanitor() => Type = AIType.Janitor;

        public void AssignHallway(HallwayColor hallwayColor) => AssignedHallway = hallwayColor;

        public override void Spawned()
        {
            interacter.Activated = false;

            if (Object.HasStateAuthority)
            {
                if (brainToAddOnSpawned)
                    AddBrain(brainToAddOnSpawned);

                Agent.enabled = true;
                interacter.Activated = true;

                if (Inventory)
                    Inventory.OnInventoryChanged += OnInventoryChanged;
            }

            RegisterToManager();
            InitializeRagdoll();
            OnAISpawned?.Invoke(this);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            UnregisterToManager();
            OnAIDespawned?.Invoke(this);

            if (Inventory)
                Inventory.OnInventoryChanged -= OnInventoryChanged;
        }

        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();
            if (isRagdoll)
            {
                // ReSharper disable Unity.InefficientPropertyAccess
                transform.position = Vector3.MoveTowards(transform.position, ragdollTransform.position.Flat(), 0.1f);
                ragdollTransform.position = Vector3.MoveTowards(ragdollTransform.position, transform.position, 0.1f);
            }

            if (!IsImmune && !networkAnimator.Animator.enabled)
                networkAnimator.Animator.enabled = true;

            UpdateWalkingAnimation();
        }

        private void UpdateWalkingAnimation()
        {
            if (!Object.HasStateAuthority)
                return;

            if (!Animator)
                return;

            Animator.SetFloat(SpeedParam, Velocity.sqrMagnitude / (MaxSpeed * MaxSpeed));
        }

        private void OnInventoryChanged()
        {
            if (!Object.HasStateAuthority)
                return;

            if (!Animator)
                return;

            if (IsJanitor || IsTeacher) return;
            
            Animator.SetBool(IsHoldingHomeworkParam, Inventory.HasHomework);
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
            Debug.Assert(brain,
                $"Calling {nameof(AddBrain)} with a prefab that doesn't have a script {nameof(AIBrain)}");

            brain.AssignEntity(this);
        }

        private void InitializeRagdoll()
        {
            if (Type != AIType.Student)
                return;

            var collidersInAIEntity = GetComponentsInChildren<Collider>();
            foreach (Collider col in collidersInAIEntity)
            {
                if (col.transform.gameObject != transform.gameObject)
                {
                    Transform colTransform = col.transform;
                    BodyPart bp = new BodyPart
                    {
                        Collider = col,
                        Position = colTransform.localPosition,
                        Rotation = colTransform.localRotation
                    };
                    ragdollColliders.Add(bp);
                    col.isTrigger = true;
                    col.attachedRigidbody.isKinematic = true;
                }
            }
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        private void RPC_ToggleRagdoll(NetworkBool isActivate, Vector3 forceDirection = default,
            float forceMagnitude = 0)
        {
            if (Type != AIType.Student)
                return;

            isRagdoll = isActivate;

            if (Object.HasStateAuthority)
                agent.enabled = !isActivate;
            Animator.enabled = false;

            if (aiCollider)
                aiCollider.enabled = !isActivate;

            foreach (BodyPart bp in ragdollColliders)
            {
                bp.Collider.isTrigger = !isActivate;
                bp.Rb.isKinematic = !isActivate;

                if (isActivate && forceDirection != default)
                    bp.Rb.AddForce(
                        forceDirection.normalized * (forceMagnitude == 0 ? Velocity.magnitude : forceMagnitude),
                        ForceMode.Impulse);
            }
        }

        public void Hit(GameObject hitter, float overrideHitDuration = -1)
        {
            // Teachers cannot be hit
            if (IsTeacher)
                return;

            if (IsImmune) return;

            if (hitCoroutine != null)
            {
                StopHitRoutine();
            }

            RPC_ToggleRagdoll(true, (transform.position - hitter.transform.position).Flat(), 10f);
            OnHit?.Invoke(hitter);
            hitCoroutine = StartCoroutine(HitRoutine(overrideHitDuration));
        }

        private IEnumerator HitRoutine(float overrideHitDuration)
        {
            var secondsToWait = overrideHitDuration > 0f ? overrideHitDuration : settings.SecondsDownAfterBeingHit;
            yield return Helpers.GetWait(secondsToWait);

            RPC_ToggleRagdoll(false);

            Animator.enabled = true;
            if (ragdollTransform)
            {
                var isGettingUpBackDown = Vector3.Dot(ragdollTransform.forward, Vector3.up) > 0;
                if (networkAnimator)
                    networkAnimator.SetTrigger(isGettingUpBackDown ? GetUpBackDownParam : GetUpFaceDownParam);
                StartCoroutine(AfterGetUp(isGettingUpBackDown));
            }
        }

        private IEnumerator AfterGetUp(bool isGettingUpBackDown)
        {
            yield return Helpers.GetWait(isGettingUpBackDown ? 0.6f : 0.533f);
            foreach (BodyPart bp in ragdollColliders)
            {
                Transform elementTransform = bp.Collider.transform;
                elementTransform.localPosition = bp.Position;
                elementTransform.localRotation = bp.Rotation;
            }

            IsImmune = false;
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

        public void PlayAlert()
        {
            RPC_PlayAlertSoundOnAllClients();
            RPC_PlayAlertParticleEffectOnAllClients();
        }

        public void StopAlert()
        {
            RPC_StopAlertParticleEffectOnAllClients();
        }

        public void PlayFootstepSoundLocally() => SoundSystem.Instance.PlayFootstepSound(this);
        public void PlayFumbleSoundLocally() => SoundSystem.Instance.PlayFumbleSound(this);
        public void PlayHandInHomeworkSoundOnAllClients() => RPC_PlayHandInHomeworkSoundOnAllClients();
        public void PlayPickUpHomeworkSoundOnAllClients() => RPC_PlayPickUpHomeworkSoundOnAllClients();

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_PlayHandInHomeworkSoundOnAllClients() => SoundSystem.Instance.PlayHandInHomeworkSound(this);

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_PlayPickUpHomeworkSoundOnAllClients() => SoundSystem.Instance.PlayPickUpHomeworkSound(this);

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_PlayAlertSoundOnAllClients() => SoundSystem.Instance.PlayJanitorCaughtAlertSound(this);

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_PlayAlertParticleEffectOnAllClients()
        {
            if (alertParticleEffect)
            {
                alertParticleEffect.Play();
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_StopAlertParticleEffectOnAllClients()
        {
            if (alertParticleEffect)
            {
                alertParticleEffect.Stop();
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!Application.isPlaying)
                EditorApplication.delayCall += AssignTagAndLayer;
        }

        private void AssignTagAndLayer()
        {
            if (this == null)
                return;

            var thisGameObject = gameObject;

            if (!thisGameObject.AssignTagIfDoesNotHaveIt(Tags.AI))
                Debug.LogWarning(
                    $"Player {thisGameObject.name} should have the tag {Tags.AI}. Instead, it has {thisGameObject.tag}");

            if (!thisGameObject.AssignLayerIfDoesNotHaveIt(Layers.AI))
                Debug.LogWarning(
                    $"Player {thisGameObject.name} should have the layer {Layers.AI} ({Layers.NAME_AI}). Instead, it has {thisGameObject.layer}");
        }

        private void Update()
        {
            var path = Agent.path;
            if (Object && IsJanitor)
            {
                Debug.Log($"Status: {path.status}; Path: {string.Join(", ",path.corners)}");
            }
        }

        private void OnDrawGizmos()
        {
            if (!Agent.enabled)
                return;

            var path = Agent.path;
            for (var i = 0; i < path.corners.Length - 1; ++i)
            {
                var one = path.corners[i];
                var two = path.corners[i + 1];
                Gizmos.DrawLine(one, two);
            }
        }
#endif
    }
}