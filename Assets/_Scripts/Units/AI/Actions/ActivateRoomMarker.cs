using System;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Managers.Rooms;
using UnityEngine;
using Action = BehaviorDesigner.Runtime.Tasks.Action;

namespace Units.AI.Actions
{
    [Serializable]
    [TaskCategory("Room")]
    [TaskDescription("Activates a room marker")]
    public class ActivateRoomMarker : Action
    {
        [SerializeField] private SharedTransform roomTarget;
        [SerializeField] private SharedFloat secondsOfActivation = 2f;

        public override TaskStatus OnUpdate()
        {
            if (!roomTarget.Value)
                return TaskStatus.Failure;

            var room = roomTarget.Value.GetComponent<Room>();
            if (!room)
                return TaskStatus.Failure;
            
            room.ActivateRoomMarker(secondsOfActivation.Value);
            return TaskStatus.Success;
        }

        public override void OnReset()
        {
            base.OnReset();
            roomTarget = null;
            secondsOfActivation = 2f;
        }
    }
}