using Ingredients.Homework;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Systems.Settings
{
    [CreateAssetMenu(menuName = "Settings/HUD Settings")]
    public class HUDSettings : ScriptableObject
    {
        [Tooltip("Opacity for deactivated actions")]
        [SerializeField] [MinValue(0.1f)] private float deactivatedActionOpacity = 0.3f;

        [Header("Score feedback")]
        [Tooltip("Tier one score feedback")]
        [SerializeField] private HomeworkDefinitionToVFX tierOneScore;
        [Tooltip("Tier two score feedback")]
        [SerializeField] private HomeworkDefinitionToVFX tierTwoScore;
        [Tooltip("Tier three score feedback")]
        [SerializeField] private HomeworkDefinitionToVFX tierThreeScore;
       
        [Tooltip("Loosing score feedback.")]
        [SerializeField] private GameObject descoreLeft;
        [SerializeField] private GameObject descoreRight;

        public float DeactivatedActionOpacity => deactivatedActionOpacity;
        public HomeworkDefinitionToVFX TierOneScore => tierOneScore;
        public HomeworkDefinitionToVFX TierTwoScore => tierTwoScore;
        public HomeworkDefinitionToVFX TierThreeScore => tierThreeScore;
        public GameObject DescoreLeft => descoreLeft;
        public GameObject DescoreRight => descoreRight;
    }
}