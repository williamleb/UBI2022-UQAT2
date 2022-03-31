using Sirenix.OdinInspector;
using UnityEngine;

namespace Systems.Settings
{
    [CreateAssetMenu(menuName = "Settings/Game Settings")]
    public class GameSettings : ScriptableObject 
    {
        [Space]
        [Header("End game")]
        [SerializeField, MinValue(1)] [Tooltip("Maximum score")] private int numberOfHomeworksToFinishGame = 15;

        [SerializeField, MinValue(1)] [Tooltip("Maximum game time in seconds")] private int gameDurationInSeconds = 300;

        [Space]
        [Header("Overtime")]
        [SerializeField] [Tooltip("Enable overtime")] private bool enableOvertime = true;
        [SerializeField] [Tooltip("Maximum duration of overtime in seconds")] private int overtimeDurationInSeconds = 30;
        [Header("Colors")]
        [SerializeField] private Color victoryColor = Color.yellow;
        [SerializeField] private Color defeatColor = Color.red;
        [SerializeField] private Color drawColor = Color.grey;
        [SerializeField] private Color replayReadyColor = Color.green;
        
        public int NumberOfHomeworksToFinishGame => numberOfHomeworksToFinishGame;
        public int GameDurationInSeconds => gameDurationInSeconds;
        public bool EnableOvertime => enableOvertime;
        public int OvertimeDurationInSeconds => overtimeDurationInSeconds;

        public Color VictoryColor => victoryColor;
        public Color DefeatColor => defeatColor;
        public Color DrawColor => drawColor;
        public Color ReplayReadyColor => replayReadyColor;
    }
}