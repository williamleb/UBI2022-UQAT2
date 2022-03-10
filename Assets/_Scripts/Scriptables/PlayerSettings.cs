using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Scriptables
{
    [CreateAssetMenu(menuName = "Settings/Player Settings")]
    public class PlayerSettings : ScriptableObject
    {
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
        private float sprintFumbleThreshold = 0.75f;

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

        [Tooltip("Time in MS that the player is knockOut either by fumble or getting hit. " +
                 "This is increased by the speed of the player. The faster you go the longer the knockout time")]
        [SerializeField]
        private int knockOutTimeInMS = 500;

        [Tooltip("The max distance from the player another entity can be to apply aim assist")] [SerializeField]
        private float dashMaxAimAssistRange = 2f;

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

        [Space]
        [Header("Other settings")]
        [Tooltip("Time in seconds before a player can be tackled again")]
        [SerializeField]
        private int immunityTime = 2;

        [SerializeField] private PlayerCameraSettings playerCameraSettings;

        public float MoveMaximumSpeed => moveMaximumSpeed;
        public float SprintMaximumSpeed => sprintMaximumSpeed;
        public float MoveAcceleration => moveAcceleration;
        public float MoveDeceleration => moveDeceleration;
        public float DashForce => dashForce;
        public float DashDuration => dashDuration;
        public int KnockOutTimeInMS => knockOutTimeInMS;
        public float TurnRotationSpeed => turnRotationSpeed;
        public float SprintBraking => sprintBraking;
        public float SprintAcceleration => sprintAcceleration;
        public PlayerCameraSettings PlayerCameraSetting => playerCameraSettings;
        public float SprintFumbleThreshold => sprintFumbleThreshold;
        public float DashMaxAimAssistRange => dashMaxAimAssistRange;
        public float DashAimAssistAngle => dashAimAssistAngle;
        public float DashAimAssistForce => dashAimAssistForce;
        public float DashCoolDown => dashCoolDown;
        public int ImmunityTime => immunityTime;

        [Serializable]
        public class PlayerCameraSettings
        {
            public float PosX;
            public float PosY;
            public float PosZ;
            public float RotX;
            public float FieldOfView;
        }
    }
}