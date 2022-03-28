﻿using Sirenix.OdinInspector;
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
        [SerializeField] [Tooltip("Activates the modulation of the overtime flow according to the game situation.")] private bool enableOvertimeModulation = true;
        [SerializeField] [Tooltip("Secondary time flow rate. Only used when overtime modulation is enabled. (e.g. when the losing team does not have in its possession an homework.")] private int secondaryTimeFlowRate = 2;

        [Header("Colors")]
        [SerializeField] private Color victoryColor = Color.green;
        [SerializeField] private Color defeatColor = Color.red;
        [SerializeField] private Color drawColor = Color.grey;
        
        public int NumberOfHomeworksToFinishGame => numberOfHomeworksToFinishGame;
        public int GameDurationInSeconds => gameDurationInSeconds;
        public bool EnableOvertime => enableOvertime;
        public int OvertimeDurationInSeconds => overtimeDurationInSeconds;
        public bool EnableOvertimeModulation => enableOvertimeModulation;
        public int SecondaryTimeFlowRate => secondaryTimeFlowRate;

        public Color VictoryColor => victoryColor;
        public Color DefeatColor => defeatColor;
        public Color DrawColor => drawColor;
    }
}