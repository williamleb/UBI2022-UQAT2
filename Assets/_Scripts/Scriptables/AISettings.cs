using Sirenix.OdinInspector;
using UnityEngine;

namespace Scriptables
{
    [CreateAssetMenu(menuName = "Settings/AI Settings")]
    public class AISettings : ScriptableObject
    {
        [Header("Teacher settings")] 
        [SerializeField, MinValue(0.1f)] private float visionNear = 1f;
        [SerializeField, MinValue(0.2f)] private float visionFar = 10f;
        [SerializeField, MinValue(0.1f)] private float visionNearLength = 2f;
        [SerializeField, MinValue(0.2f)] private float visionFarLength = 20f;
        [SerializeField, ReadOnly] private float secondsOfImmobilisationBadBehavior = 1.5f; // Not yet implemented
        [SerializeField, ReadOnly] private float secondsOfImmobilisationFakeHomework = 1.5f; // Not yet implemented
        [SerializeField, MinMaxSlider(0f, 30f, true)] private Vector2 secondsToStayInARoom = new Vector2(10f, 15f);
        [SerializeField, MinMaxSlider(0f, 30f, true)] private Vector2 secondsToChangePositionsInARoom = new Vector2(3f, 5f);
        [SerializeField] private float secondsToShowRoomIndicator = 2f;
        [SerializeField] private float secondsToChaseBadBehavior = 5f;
        
       public float VisionNear => visionNear;
       public float VisionFar => visionFar;
       public float VisionNearLength => visionNearLength;
       public float VisionFarLength => visionFarLength;
       public float SecondsOfImmobilisationBadBehavior => secondsOfImmobilisationBadBehavior;
       public float SecondsOfImmobilisationFakeHomework => secondsOfImmobilisationFakeHomework;
       public float MinSecondsToStayInARoom => secondsToStayInARoom.x;
       public float MaxSecondsToStayInARoom => secondsToStayInARoom.y;
       public float MinSecondsToChangePositionInARoom => secondsToChangePositionsInARoom.x;
       public float MaxSecondsToChangePositionInARoom => secondsToChangePositionsInARoom.y;
       public float SecondsToShowRoomIndicator => secondsToShowRoomIndicator;
       public float SecondsToChaseBadBehavior => secondsToChaseBadBehavior;
    }
}