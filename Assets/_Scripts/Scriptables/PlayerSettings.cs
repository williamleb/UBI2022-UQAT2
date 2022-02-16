using UnityEngine;

namespace Scriptables
{
    [CreateAssetMenu(menuName = "Settings/Player Settings")]
    public class PlayerSettings : ScriptableObject
    {
        [Header("Player movement config")] [SerializeField]
        private float moveMaximumSpeed = 13f;

        [SerializeField] private float maxFallSpeed = -40f;
        [SerializeField] private float moveAcceleration = 90f;
        [SerializeField] private float moveDeceleration = 60f;
        [SerializeField] private float minFallAcceleration = 80f;
        [SerializeField] private float maxFallAcceleration = 120f;

        [Space] [Header("Mouse settings")] [SerializeField]
        private float mouseSensitivity = 0.5f;

        [Space] [Header("Jump settings")] [SerializeField]
        private float jumpHeight = 30f;

        [Tooltip("Amount of time after leaving the platform to still allow player to jump")] [SerializeField]
        private float jumpCoyoteTimeThreshold = 0.1f;

        [Tooltip("Increases the range of height which is considered the peak of the jump")] [SerializeField]
        private float jumpApexThreshold = 10f;

        [Tooltip("Gives more movement at the peak of the jump")] [SerializeField]
        private float moveAirBonusControl = 2f;

        [Tooltip("Gravity multiplier when you let go of jump button")] [SerializeField]
        private float jumpEndEarlyGravityModifier = 3f;

        #region accessors

        public float MoveMaximumSpeed => moveMaximumSpeed;
        public float MoveAcceleration => moveAcceleration;
        public float MoveDeceleration => moveDeceleration;
        public float MoveAirBonusControl => moveAirBonusControl;
        public float MouseSensitivity => mouseSensitivity;
        public float JumpHeight => jumpHeight;
        public float MinFallAcceleration => minFallAcceleration;
        public float MaxFallAcceleration => maxFallAcceleration;
        public float MaxFallSpeed => maxFallSpeed;
        public float JumpCoyoteTimeThreshold => jumpCoyoteTimeThreshold;
        public float JumpApexThreshold => jumpApexThreshold;
        public float JumpEndEarlyGravityModifier => jumpEndEarlyGravityModifier;

        #endregion
    }
}