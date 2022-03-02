using System.Linq;
using BehaviorDesigner.Runtime;
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
        [SerializeField] private SharedTransform roomTarget;
        
        private Room roomToWalkTo;
        private Vector3 randomPositionInRoom;

        protected override Vector3 Destination => randomPositionInRoom;
        protected override bool UpdateDestination => roomToWalkTo != null;
        protected override bool SetDestinationOnStart => roomToWalkTo != null;
        
        protected override void OnBeforeStart()
        {
            base.OnBeforeStart();

            if (roomTarget.Value == null)
            {
                SetNewDestination();
            }
            else
            {
                RestoreDestination();
            }
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
            roomTarget?.SetValue(roomToWalkTo.transform);
            randomPositionInRoom = roomToWalkTo.GetRandomRoomPosition();
        }

        private void RestoreDestination()
        {
            roomToWalkTo = roomTarget.Value.GetComponent<Room>();
            Debug.Assert(roomToWalkTo);
            randomPositionInRoom = roomToWalkTo.GetRandomRoomPosition();
        }

        public override void OnEnd()
        {
            roomToWalkTo = null;
            randomPositionInRoom = Vector3.zero;
            
            base.OnEnd();
        }

        public override void OnReset()
        {
            base.OnReset();
            roomTarget = null;
        }
    }
}