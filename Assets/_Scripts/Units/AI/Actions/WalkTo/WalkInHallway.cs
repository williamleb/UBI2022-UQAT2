using System;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Managers.Hallway;
using Managers.Interactions;
using UnityEngine;
using Utilities.Extensions;

namespace Units.AI.Actions
{
    [Serializable]
    [TaskCategory("AI/Walk To")]
    [TaskDescription("Walk in the hallway.")]
    public class WalkInHallway : WalkTo
    {
        [SerializeField] private SharedBool endsOnFirstHallwayPointReached = false;
        [SerializeField] private SharedFloat distanceFromHallwayPointToFinish = 2f;
        
        private Hallway hallwayToWalkIn;
        private HallwayPoint hallwayPointToWalkTo;

        protected override Vector3 Destination => hallwayPointToWalkTo.transform.position;
        protected override bool EndsOnDestinationReached => endsOnFirstHallwayPointReached.Value;
        protected override bool UpdateDestination => hallwayPointToWalkTo != null;
        protected override bool SetDestinationOnStart => hallwayPointToWalkTo != null;
        
        protected virtual bool FilterInteraction(Interaction interaction) => interaction;

        protected override void OnBeforeStart()
        {
            base.OnBeforeStart();
            InitHallwayToWalkIn();
        }

        protected override TaskStatus OnUpdateImplementation()
        {
            if (!hallwayPointToWalkTo)
                return TaskStatus.Failure;

            if (Brain.Position.SqrDistanceWith(Destination) < distanceFromHallwayPointToFinish.Value * distanceFromHallwayPointToFinish.Value)
            {
                if (endsOnFirstHallwayPointReached.Value)
                    return TaskStatus.Success;
                
                SeekNextPoint();
            }
            
            Brain.SetSpeed(Brain.BaseSpeed - hallwayToWalkIn.GetProgress(hallwayPointToWalkTo, Brain.Position) * 2.5f);

            return TaskStatus.Running;
        }

        private void SeekNextPoint()
        {
            if (!HallwayManager.HasInstance)
                return;

            hallwayPointToWalkTo = hallwayToWalkIn.GetNextPoint(hallwayPointToWalkTo);
        }
        
        private void InitHallwayToWalkIn()
        {
            if (!HallwayManager.HasInstance)
                return;

            hallwayToWalkIn = HallwayManager.Instance.GetRandomHallway();
            if (!hallwayToWalkIn)
                return;
                
            hallwayPointToWalkTo = hallwayToWalkIn.GetClosestPointTo(Brain.Position);
        }

        public override void OnEnd()
        {
            hallwayToWalkIn = null;
            hallwayPointToWalkTo = null;
            
            Brain.ResetSpeed();
            
            base.OnEnd();
        }

        public override void OnReset()
        {
            base.OnReset();
            endsOnFirstHallwayPointReached = false;
        }
    }
}