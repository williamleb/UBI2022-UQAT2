using System;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Systems.Settings;
using UnityEngine;

namespace Units.AI.Actions
{
    [Serializable]
    [TaskCategory("AI/Load Settings")]
    public class LoadJanitorSettings : AIAction
    {
        [SerializeField] private SharedFloat secondsToChaseBadBehavior;
        [SerializeField] private SharedFloat chaseBadBehaviorSpeed;
        [SerializeField] private SharedFloat secondsOfImmobilizationBadBehavior;
        [SerializeField] private SharedInt badBehaviorPointsToLose;

        private AISettings settings = null;

        public override void OnStart()
        {
            base.OnStart();

            if (!SettingsSystem.HasInstance)
                return;

            settings = SettingsSystem.AISettings;

            secondsToChaseBadBehavior?.SetValue(settings.SecondsToChaseBadBehavior);
            chaseBadBehaviorSpeed?.SetValue(settings.ChaseBadBehaviorSpeed);
            secondsOfImmobilizationBadBehavior?.SetValue(settings.SecondsOfImmobilisationBadBehavior);
            badBehaviorPointsToLose?.SetValue(settings.BadBehaviorPointsToLose);
        }

        public override TaskStatus OnUpdate()
        {
            return settings == null ? TaskStatus.Failure : TaskStatus.Success;
        }

        public override void OnReset()
        {
            base.OnReset();
            secondsToChaseBadBehavior = null;
            chaseBadBehaviorSpeed = null;
            secondsOfImmobilizationBadBehavior = null;
            badBehaviorPointsToLose = null;
        }
    }
}