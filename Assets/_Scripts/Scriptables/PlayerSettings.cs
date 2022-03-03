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

        [Space]
        [Header("Turn config")]
        [Tooltip("VISUAL ONLY Rotation step to turn towards new direction, " +
                 "higher means bigger steps faster snap to new rotation")]
        [SerializeField]
        private float turnRotationSpeed = 0.2f;

        [Tooltip("Turn rate the factor that divides the move acceleration when turning. Higher means slower turns")]
        [SerializeField]
        [MinValue(1.1f)]
        private float turnRate = 2f;

        [Space]
        [Header("Dash configuration")]
        [Tooltip("Amount of force of the dash. Higher number means you dash at a higher speed")]
        [SerializeField]
        private float dashForce = 30f;

        [Tooltip("Time in seconds before the dash has ended and we assumed the player missed and fumbles")]
        [SerializeField]
        private float dashDuration = 0.3f;

        [Tooltip("Time in MS that the player is knockOut either by fumble or getting hit. " +
                 "This is increased by the speed of the player. The faster you go the longer the knockout time")]
        [SerializeField]
        private int knockOutTimeInMS = 500;

        [SerializeField] private PlayerCameraSettings playerCameraSettings;

        public float MoveMaximumSpeed => moveMaximumSpeed;
        public float SprintMaximumSpeed => sprintMaximumSpeed;
        public float MoveAcceleration => moveAcceleration;
        public float MoveDeceleration => moveDeceleration;
        public float DashForce => dashForce;
        public float DashDuration => dashDuration;
        public int KnockOutTimeInMS => knockOutTimeInMS;
        public float TurnRotationSpeed => turnRotationSpeed;
        public float TurnRate => turnRate;
        public float SprintBraking => sprintBraking;
        public float SprintAcceleration => sprintAcceleration;
        public PlayerCameraSettings PlayerCameraSetting => playerCameraSettings;
        
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