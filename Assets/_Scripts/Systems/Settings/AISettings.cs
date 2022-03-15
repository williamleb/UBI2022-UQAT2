using Sirenix.OdinInspector;
using UnityEngine;

namespace Systems.Settings
{
    [CreateAssetMenu(menuName = "Settings/AI Settings")]
    public class AISettings : ScriptableObject
    {
        [Header("Teacher settings")]
        [SerializeField, MinMaxSlider(0f, 30f, true)] private Vector2 secondsToStayInARoom = new Vector2(10f, 15f);
        [SerializeField, MinMaxSlider(0f, 30f, true)] private Vector2 secondsToChangePositionsInARoom = new Vector2(3f, 5f);
        [SerializeField] private float secondsToShowRoomIndicator = 2f;
        [SerializeField] private float baseTeacherSpeed = 3.5f;

        [Header("Janitor settings")]
        [SerializeField, MinValue(0.1f)] private float visionNear = 1f;
        [SerializeField, MinValue(0.2f)] private float visionFar = 10f;
        [SerializeField, MinValue(0.1f)] private float visionNearLength = 2f;
        [SerializeField, MinValue(0.2f)] private float visionFarLength = 20f;
        [SerializeField, ReadOnly] private float secondsOfImmobilisationBadBehavior = 1.5f; // Not yet implemented
        [SerializeField] private float secondsToChaseBadBehavior = 5f;
        [SerializeField] private float baseJanitorSpeed = 3.5f;
        [SerializeField] private float chaseBadBehaviorSpeed = 7f;

        [Header("Student settings")] 
        [SerializeField] private float secondsDownAfterBeingHit = 5f;
        [Tooltip("Seconds between the moment a homework is placed in the world and the moment an AI is made aware of its existence (so they can go fetch it)")]
        [SerializeField, MinMaxSlider(0f, 30f, true)] private Vector2 secondsToNoticeHomework = new Vector2(5f, 10f);
        [SerializeField] private float baseStudentSpeed = 3.5f;
        [Tooltip("Variation of speed of the student depending on its position compared to the center of the group (the position is in percents between -0.5f and 0.5f)")]
        [ValidateInput(nameof(ValidateVariationOfSpeed), "All times should be between 0 and 1")]
        [SerializeField] private AnimationCurve variationOfSpeedBasedOnPositionComparedToGroup = new AnimationCurve(new Keyframe(-0.5f, 1f), new Keyframe(0f, 0f), new Keyframe(0.5f, -1f));

        public float MinSecondsToStayInARoom => secondsToStayInARoom.x;
        public float MaxSecondsToStayInARoom => secondsToStayInARoom.y;
        public float MinSecondsToChangePositionInARoom => secondsToChangePositionsInARoom.x;
        public float MaxSecondsToChangePositionInARoom => secondsToChangePositionsInARoom.y;
        public float SecondsToShowRoomIndicator => secondsToShowRoomIndicator;
        public float BaseTeacherSpeed => baseTeacherSpeed;
        
        public float VisionNear => visionNear;
        public float VisionFar => visionFar;
        public float VisionNearLength => visionNearLength;
        public float VisionFarLength => visionFarLength;
        public float SecondsOfImmobilisationBadBehavior => secondsOfImmobilisationBadBehavior;
        public float SecondsToChaseBadBehavior => secondsToChaseBadBehavior;
        public float BaseJanitorSpeed => baseJanitorSpeed;
        public float ChaseBadBehaviorSpeed => chaseBadBehaviorSpeed;

        public float SecondsDownAfterBeingHit => secondsDownAfterBeingHit;
        public float MinSecondsToNoticeHomework => secondsToNoticeHomework.x;
        public float MaxSecondsToNoticeHomework => secondsToNoticeHomework.y;
        public float BaseStudentSpeed => baseStudentSpeed;
        public AnimationCurve VariationOfSpeedBasedOnPositionComparedToGroup => variationOfSpeedBasedOnPositionComparedToGroup;

        private bool ValidateVariationOfSpeed()
        {
            foreach (var key in variationOfSpeedBasedOnPositionComparedToGroup.keys)
            {
                if (key.time < -0.5f || key.time > 0.5f)
                    return false;
            }

            return true;
        }
    }
}