﻿using Sirenix.OdinInspector;
using UnityEngine;

namespace Systems.Settings
{
    [CreateAssetMenu(menuName = "Settings/Homework Settings")]
    public class HomeworkSettings : ScriptableObject
    {
        [Header("Spawning")]
        [SerializeField, MinValue(0f), MinMaxSlider(0f, 30f, true)] private Vector2 secondsBeforeHomeworkSpawn = new Vector2(10f, 20f);
        [SerializeField, MinValue(0)] private int maxNumberOfHomeworksInPlay = 3;

        [Header("Falling")] 
        [SerializeField, MinValue(0f)] private float currentObjectContributionToHomeworkFalling = 32f;
        [SerializeField, MinValue(0f)] private float impactContributionToHomeworkFalling = 64f;

        public float MinSecondsBeforeHomeworkSpawn => secondsBeforeHomeworkSpawn.x;
        public float MaxSecondsBeforeHomeworkSpawn => secondsBeforeHomeworkSpawn.y;
        public int MaxNumberOfHomeworksInPlay => maxNumberOfHomeworksInPlay;

        public float CurrentObjectContributionToHomeworkFalling => currentObjectContributionToHomeworkFalling;
        public float ImpactContributionToHomeworkFalling => impactContributionToHomeworkFalling;
    }
}