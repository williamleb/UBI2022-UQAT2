using System.Linq;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Managers.Rooms;
using UnityEngine;
using Utilities.Extensions;

namespace Units.AI.Actions
{
    [TaskCategory("AI/Walk To")]
    [TaskDescription("Make the AI walk towards a random point in the target room. Needs a target room to be assigned by the action WalkToRandomRoom or else it will fail.")]
    public class WalkToRandomPointInTargetRoom : WalkTo
    {
        [SerializeField] private SharedTransform roomTarget;

        private Room currentRoom;
        private Vector3 randomPositionInRoom;

        protected override Vector3 Destination => randomPositionInRoom;
        protected override bool UpdateDestination => roomTarget != null;
        protected override bool SetDestinationOnStart => roomTarget != null;
        
        protected override void OnBeforeStart()
        {
            base.OnBeforeStart();
            SetNewDestination();
        }

        protected override TaskStatus OnUpdateImplementation()
        {
            if (roomTarget == null)
                return TaskStatus.Failure;

            return TaskStatus.Running;
        }
        
        private void SetNewDestination()
        {
            if (roomTarget == null)
                return;

            currentRoom = roomTarget.Value.GetComponent<Room>();
            if (!currentRoom)
                return;
            
            randomPositionInRoom = currentRoom.GetRandomRoomPosition();
        }

        public override void OnEnd()
        {
            currentRoom = null;
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