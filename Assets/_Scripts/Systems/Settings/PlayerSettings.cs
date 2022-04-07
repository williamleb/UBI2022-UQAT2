using System;
using Sirenix.OdinInspector;
using Units.Player;
using UnityEngine;
using UnityEngine.Serialization;

namespace Systems.Settings
{
    [CreateAssetMenu(menuName = "Settings/Player Settings")]
    public class PlayerSettings : ScriptableObject
    {
        [FormerlySerializedAs("archetypes")] [SerializeField] private Archetype archetype;
        
        [Header("Player movement ")] [Tooltip("Walking speed")] [SerializeField]
        private float moveMaximumSpeed = 7f;

        [Tooltip("Gaz, the amount of force added when moving. Higher number means you reach max speed faster")]
        [SerializeField]
        private float moveAcceleration = 100f;

        [Tooltip("Braking, the amount of force loss when not moving. Higher number means you stop faster")]
        [SerializeField]
        private float moveDeceleration = 7f;

        [Space] [Header("Sprint config")] [Tooltip("Sprint max speed")] [SerializeField]
        private float sprintMaximumSpeed = 14f;

        [Tooltip("time to sprint maximum speed, increases max speed per second. " +
                 "Higher number means you reach sprint max speed faster")]
        [SerializeField]
        private float sprintAcceleration = 20f;

        [Tooltip("time to normal, decreases max speed per second. Higher number means your speed decreases faster")]
        [SerializeField]
        private float sprintBraking = 10f;

        [MinValue(0)] [MaxValue(1)] [SerializeField]
        [Tooltip("% of the sprint maximum speed the player need to go to trigger a fumble")]
        private float sprintFumbleThreshold = 0.75f;

        [SerializeField]
        [Tooltip("Allows the class to fumble when hitting a wall or other AI")]
        private bool canFumble = true;
        
        [Tooltip("Time in seconds that the player is knockOut either by fumble" +
                 "This is increased by the speed of the player. The faster you go the longer the knockout time")]
        [SerializeField]
        private float fumbleKnockOutTimeInSeconds = 0.5f;

        [Space]
        [Header("Turn config")]
        [Tooltip("Rotation speed to turn towards new direction, " +
                 "higher means bigger steps faster snap to new rotation" +
                 "This number is divided by the player's velocity thus increasing the turn radius at high speed.")]
        [SerializeField]
        private float turnRotationSpeed = 10f;

        [Space]
        [Header("Dash configuration")]
        [Tooltip("Amount of force of the dash. Higher number means you dash at a higher speed")]
        [SerializeField]
        private float dashForce = 30f;

        [Tooltip("Time in seconds before the dash has ended and we assumed the player missed and fumbles")]
        [SerializeField]
        private float dashDuration = 0.3f;

        [Tooltip("Time in seconds before the player can dash again")] [SerializeField]
        private float dashCoolDown = 1.5f;

        [Tooltip("Time in seconds that the player is knockOut either by getting hit. " +
                 "This is increased by the speed of the player. The faster you go the longer the knockout time")]
        [SerializeField]
        private float knockOutTimeInSeconds = 0.5f;

        [Tooltip("The max distance from the player another entity can be to apply aim assist")] [SerializeField]
        private float dashMaxAimAssistRange = 2f;

        [Tooltip("Force applied on the ragdoll of the tackled entity following a tackle")]
        [SerializeField]
        private float dashForceApplied = 30f;

        [Tooltip(
            "The target angle in front of the player in degrees on each side of the direction of the player (45° means a full 90° of view angle)")]
        [SerializeField]
        [MinValue(0)]
        [MaxValue(180)]
        private float dashAimAssistAngle = 45f;

        [Tooltip("The amount of correction applied to the dash direction [0,1]")]
        [SerializeField]
        [MinValue(0)]
        [MaxValue(1)]
        private float dashAimAssistForce = 0.5f;
        [Tooltip("The size of the detection sphere in front of the player. If set too big, it will detect walls on the player's sides instead of only in front")]
        [SerializeField]
        [MinValue(0)]
        private float dashDetectionSphereRadius = 0.5f;
        [SerializeField] [Tooltip("Can multi hit with the dash")]
        private bool canMultiHitWithDash;
        [SerializeField] [Tooltip("Allows the class to fumble at the end of the dash if they didn't hit anyone")] 
        private bool ragdollOnFailedDashEnd;
        
        [Header("On successful dash")]
        [SerializeField] [Tooltip("Allows the class to fumble at the end of the dash even if they hit someone")] 
        private bool ragdollOnSuccessDashEnd;
        [SerializeField] [Tooltip("Allows the class to fumble when hitting a wall even if they hit someone")] 
        private bool ragdollOnSuccessDashWallHit;

        [Space] 
        [Header("Throw config")] 
        [Tooltip("The force at which the homework will be thrown by the player. A higher number means that the homework will be thrown farther and faster.")] 
        [SerializeField]
        [MinValue(0f)]
        private float maxThrowForce = 4f;
        
        [Tooltip("The min force at which the homework will be thrown by the player (e.g. if the player doesn't hold the throw button at all). A higher number means that the homework will be thrown farther and faster.")] 
        [SerializeField]
        [MinValue(0f)]
        private float minThrowForce = 1f;
        
        [Tooltip("How much the homework will be thrown vertically in the air. A value of 0 means the homework will be thrown horizontally, while a value of 1 means the homework will be thrown at an angle of 45 degrees.")] 
        [SerializeField]
        [MinValue(0f)]
        private float throwVerticality = 0.1f;
        
        [Tooltip("Number of seconds the player has to hold the throw button before it reaches full force determined by the 'maxThrowForce' variable." +
                 "The force increases linearly between 0 and maxThrowForce when the timer goes from 0 to secondsBeforeMaxThrowForce.")] 
        [SerializeField]
        [MinValue(0f)]
        private float secondsBeforeMaxThrowForce = 3f;
        
        [Tooltip("The speed at which the player should move when they are aiming before throwing.")] 
        [SerializeField]
        [MinValue(0f)]
        private float aimingMovementSpeed = 3f;
        
        [Tooltip("The acceleration at which the player will come from its regular movement speed to its aiming speed.")] 
        [SerializeField]
        [MinValue(0f)]
        private float aimingMovementDeceleration = 0.1f;
        
        [Space]
        [Header("Other settings")]
        [Tooltip("Time in seconds before a player can be tackled again")]
        [SerializeField]
        private int immunityTime = 2;

        [SerializeField] private PlayerCameraSettings playerCameraSettings;

        public Archetype PlayerArchetype => archetype;
        public float MoveMaximumSpeed => moveMaximumSpeed;
        public float SprintMaximumSpeed => sprintMaximumSpeed;
        public float MoveAcceleration => moveAcceleration;
        public float MoveDeceleration => moveDeceleration;
        public float DashForce => dashForce;
        public float DashDuration => dashDuration;
        public float KnockOutTimeInSeconds => knockOutTimeInSeconds;
        public float FumbleKnockOutTimeInSeconds => fumbleKnockOutTimeInSeconds;
        public float TurnRotationSpeed => turnRotationSpeed;
        public float SprintBraking => sprintBraking;
        public float SprintAcceleration => sprintAcceleration;
        public PlayerCameraSettings PlayerCameraSetting => playerCameraSettings;
        public float SprintFumbleThreshold => sprintFumbleThreshold;
        public float DashMaxAimAssistRange => dashMaxAimAssistRange;
        public float DashAimAssistAngle => dashAimAssistAngle;
        public float DashAimAssistForce => dashAimAssistForce;
        public float DashCoolDown => dashCoolDown;
        public float DashForceApplied => dashForceApplied;
        public float MaxThrowForce => maxThrowForce;
        public float MinThrowForce => minThrowForce;
        public float ThrowVerticality => throwVerticality;
        public float SecondsBeforeMaxThrowForce => secondsBeforeMaxThrowForce;
        public float AimingMovementSpeed => aimingMovementSpeed;
        public float AimingMovementDeceleration => aimingMovementDeceleration;
        public int ImmunityTime => immunityTime;
        public float CameraPointSpeed => playerCameraSettings.CameraPointSpeed;
        public Vector3 CameraPointOffset => playerCameraSettings.CameraPointOffset;
        public float DashDetectionSphereRadius => dashDetectionSphereRadius;
        public bool CanFumble => canFumble;
        public bool RagdollOnSuccessDashEnd => ragdollOnSuccessDashEnd;
        public bool RagdollOnSuccessDashWallHit => ragdollOnSuccessDashWallHit;
        public bool RagdollOnFailedDashEnd => ragdollOnFailedDashEnd;
        public bool CanMultiHitWithDash => canMultiHitWithDash;

        [Serializable]
        public class PlayerCameraSettings
        {
            public float PosX;
            public float PosY;
            public float PosZ;
            public float RotX;
            public float FieldOfView;

            [Space] public float CameraPointSpeed;
            public Vector3 CameraPointOffset;
        }
    }
}