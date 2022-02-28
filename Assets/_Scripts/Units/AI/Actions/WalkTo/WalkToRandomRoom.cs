using System.Linq;
using BehaviorDesigner.Runtime.Tasks;
using Managers.Rooms;
using UnityEngine;
using Utilities.Extensions;

namespace Units.AI.Actions
{
    [TaskCategory("AI/Walk To")]
    [TaskDescription("Make the AI walk towards a random room.")]
    public class WalkToRandomRoom : WalkTo
    {
        private Room roomToWalkTo;
        private Vector3 randomPositionInRoom;

        protected override Vector3 Destination => randomPositionInRoom;
        protected override bool UpdateDestination => roomToWalkTo != null;
        protected override bool SetDestinationOnStart => roomToWalkTo != null;
        
        protected override void OnBeforeStart()
        {
            base.OnBeforeStart();
            SetNewDestination();
        }

        protected override TaskStatus OnUpdateImplementation()
        {
            if (!roomToWalkTo)
                return TaskStatus.Failure;

            return TaskStatus.Running;
        }
        
        private void SetNewDestination()
        {
            if (!RoomManager.HasInstance)
                return;

            var rooms = RoomManager.Instance.Rooms.ToArray();
            if (!rooms.Any())
                return;

            roomToWalkTo = rooms.RandomElement();
            randomPositionInRoom = roomToWalkTo.GetRandomRoomPosition();
        }

        public override void OnEnd()
        {
            roomToWalkTo = null;
            randomPositionInRoom = Vector3.zero;
            
            base.OnEnd();
        }
    }
}