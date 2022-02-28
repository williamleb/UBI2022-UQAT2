using UnityEngine;

namespace Scriptables
{
    [CreateAssetMenu(menuName = "Settings/Player Settings")]
    public class PlayerSettings : ScriptableObject
    {
        [Header("Player movement config")] [SerializeField]
        private float moveMaximumSpeed = 7f;
        [SerializeField] private float sprintMaximumSpeed = 14f;
        [SerializeField] private float moveAcceleration = 100f;
        [SerializeField] private float moveDeceleration = 7f;
        [SerializeField] private float dashSpeed = 28f;
        [SerializeField] private float dashDuration = 0.3f;
        [SerializeField] private int knockOutTimeInMS = 500;

        #region accessors

        public float MoveMaximumSpeed => moveMaximumSpeed;
        public float SprintMaximumSpeed => sprintMaximumSpeed;
        public float MoveAcceleration => moveAcceleration;
        public float MoveDeceleration => moveDeceleration;
        public float DashSpeed => dashSpeed;
        public float DashDuration => dashDuration;
        public int KnockOutTimeInMS => knockOutTimeInMS;

        #endregion
    }
}