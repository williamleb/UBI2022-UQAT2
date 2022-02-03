using UnityEngine;

namespace Scriptables
{
    [CreateAssetMenu(menuName = "Settings/Global Settings")]
    public class GlobalSettings : ScriptableObject
    {
        [Space]
        [Header("Player movement config")]
        [SerializeField] private float moveMaximumSpeed = 13f;
        [SerializeField] private float moveAcceleration = 90f;
        [SerializeField] private float moveDeceleration = 60f;
        [Tooltip("Gives more movement at the peak of the jump")]
        [SerializeField] private float moveAirBonusControl = 2f;
        [SerializeField] private float mouseSensitivity = 0.5f;
        [SerializeField] private float jumpHeight = 30f;
        [SerializeField] private float minFallAcceleration = 80f;
        [SerializeField] private float maxFallAcceleration = 120f;
        [SerializeField] private float maxFallSpeed = -40f;
        [Tooltip("Amount of time after leaving the platform to still allow player to jump")]
        [SerializeField] private float jumpCoyoteTimeThreshold = 0.1f;
        [Tooltip("Increases the range of height which is considered the peak of the jump")]
        [SerializeField] private float jumpApexThreshold = 10f;
        [Tooltip("Gravity multiplier when you let go of jump button")]
        [SerializeField] private float jumpEndEarlyGravityModifier = 3f;

        public float[] PlayerMovementConfig => new[]
        {
            moveMaximumSpeed,
            moveAcceleration,
            moveDeceleration,
            moveAirBonusControl,
            jumpHeight,
            minFallAcceleration,
            maxFallAcceleration,
            maxFallSpeed,
            mouseSensitivity,
            jumpCoyoteTimeThreshold,
            jumpApexThreshold,
            jumpEndEarlyGravityModifier,
        };
    }
}