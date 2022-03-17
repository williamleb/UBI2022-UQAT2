using System;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Systems.Settings;
using UnityEngine;

namespace Units.AI.Actions
{
    [Serializable]
    [TaskCategory("AI/Load Settings")]
    public class LoadTeacherSettings : AIAction
    {
        [SerializeField] private SharedFloat minSecondsToStayInARoom;
        [SerializeField] private SharedFloat maxSecondsToStayInARoom;
        [SerializeField] private SharedFloat minSecondsToChangePositionInARoom;
        [SerializeField] private SharedFloat maxSecondsToChangePositionInARoom;
        [SerializeField] private SharedFloat secondsToShowRoomIndicator;

        private AISettings settings = null;

        public override void OnStart()
        {
            base.OnStart();

            if (!SettingsSystem.HasInstance)
                return;

            settings = SettingsSystem.AISettings;

            minSecondsToStayInARoom?.SetValue(settings.MinSecondsToStayInARoom);
            maxSecondsToStayInARoom?.SetValue(settings.MaxSecondsToStayInARoom);
            minSecondsToChangePositionInARoom?.SetValue(settings.MinSecondsToChangePositionInARoom);
            maxSecondsToChangePositionInARoom?.SetValue(settings.MaxSecondsToChangePositionInARoom);
            secondsToShowRoomIndicator?.SetValue(settings.SecondsToShowRoomIndicator);
        }

        public override TaskStatus OnUpdate()
        {
            return settings == null ? TaskStatus.Failure : TaskStatus.Success;
        }

        public override void OnReset()
        {
            base.OnReset();
            minSecondsToStayInARoom = null;
            maxSecondsToStayInARoom = null;
            minSecondsToChangePositionInARoom = null;
            maxSecondsToChangePositionInARoom = null;
            secondsToShowRoomIndicator = null;
        }
    }
}