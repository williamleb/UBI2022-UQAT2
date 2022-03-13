using System;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.ObjectDrawers;
using Fusion;
using Interfaces;
using Managers.Game;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace Ingredients.Homework
{
    [CreateAssetMenu(menuName = "Game/Homework Definition", fileName = "HomeworkDefinition")]
    public class HomeworkDefinition : ScriptableObject, IProbabilityObject
    {
        [Header("Identifier")] 
        [Tooltip("Name of the homework type. Must be unique")] 
        [ValidateInput(nameof(ValidateHomeworkType), "You must give a unique name to this homework type")]
        [SerializeField] private string homeworkType = "Default";
        
        [Header("Points")]
        [SerializeField] private int pointsGiven = 1;
        
        [Header("Spawn")]
        [Tooltip("Probability between 0 and 1 that this homework will be chosen to spawn. This is a weighted probability depending on other homeworks' probability value. If every homework has a probability of 0.5, they will all have an equal chance to be chosen.")]
        [ValidateInput(nameof(ValidateProbabilityValues), "All values should be between 0 and 1")]
        [ValidateInput(nameof(ValidateProbabilityTimes), "All times should be between 0 and 1")]
        [SerializeField] private AnimationCurve probabilityOverGameProgression = new AnimationCurve(new Keyframe(0f, 0.5f), new Keyframe(1f, 0.5f));
        
        [Tooltip("The maximum amount of homeworks of this kind which can be spawned in the game at the same time")]
        [SerializeField, MinValue(1)] private int maxAmountAtTheSameTime = 3;
        
        [Tooltip("Minimum number of homework that have to be spawned before this one can be spawned again")]
        [SerializeField, MinValue(0)] private int cooldown;

        [Tooltip("Force this homework to be spawned when the game reaches a certain progression. If two bursts happen at the same time, the chosen burst will be determined based on the homework's probability")]
        [SerializeField] private List<Burst> bursts = new List<Burst>();

        [Header("Prefab")]
        [SerializeField, Required] private NetworkObject homeworkPrefab;

        public string Type => homeworkType;

        public int Points => pointsGiven;
        
        public float Probability => GameManager.HasInstance
            ? probabilityOverGameProgression.Evaluate(GameManager.Instance.GameProgression)
            : probabilityOverGameProgression.Evaluate(0.5f);
        public int MaxAmountAtTheSameTime => maxAmountAtTheSameTime;
        public int Cooldown => cooldown;
        public IEnumerable<Burst> Bursts => bursts;

        public NetworkObject Prefab => homeworkPrefab;

        private bool ValidateHomeworkType()
        {
            return !homeworkType.IsNullOrWhitespace() && !homeworkType.Equals("Default");
        }

        private bool ValidateProbabilityValues()
        {
            foreach (var key in probabilityOverGameProgression.keys)
            {
                if (key.value < 0f || key.value > 1f)
                    return false;
            }

            return true;
        }
        
        private bool ValidateProbabilityTimes()
        {
            foreach (var key in probabilityOverGameProgression.keys)
            {
                if (key.time < 0f || key.time > 1f)
                    return false;
            }

            return true;
        }

        [Serializable]
        public struct Burst
        {
            [Tooltip("The probability (between 0 and 1) of this burst happening given it is chosen")]
            public float Probability;
            [Tooltip("The progress the game needs to be at for the burst to happen")]
            [MinValue(0f), MaxValue(1f)]
            public float AtProgress;
            [Tooltip("Whether or not to retry the burst if it has failed (if the probability is less than 1)")]
            public bool Retry;
        }
    }
}