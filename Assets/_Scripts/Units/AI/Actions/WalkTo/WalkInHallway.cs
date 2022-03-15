﻿using System;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Managers.Hallway;
using Managers.Interactions;
using Systems.Settings;
using UnityEngine;
using UnityEngine.UIElements;
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
        private Vector3 positionToWalkTo;
        private HallwayProgress hallwayProgress;

        protected override Vector3 Destination => positionToWalkTo;
        protected override bool EndsOnDestinationReached => endsOnFirstHallwayPointReached.Value;
        protected override bool UpdateDestination => hallwayPointToWalkTo != null;
        protected override bool SetDestinationOnStart => hallwayPointToWalkTo != null;
        
        protected virtual bool FilterInteraction(Interaction interaction) => interaction;

        protected override void OnBeforeStart()
        {
            base.OnBeforeStart();
            
            hallwayProgress = new HallwayProgress();
            InitHallwayToWalkIn();
            if (hallwayToWalkIn)
                UpdateHallwayProgress();
        }

        protected override TaskStatus OnUpdateImplementation()
        {
            if (!hallwayPointToWalkTo)
                return TaskStatus.Failure;

            if (hallwayPointToWalkTo.HasPassedThisPoint(Brain.Position) || Brain.Position.SqrDistanceWith(Destination) < distanceFromHallwayPointToFinish.Value * distanceFromHallwayPointToFinish.Value)
            {
                if (endsOnFirstHallwayPointReached.Value)
                    return TaskStatus.Success;
                
                SeekNextPoint();
            }
            
            UpdateHallwayProgress();
            var progressInRelationToGroup = hallwayToWalkIn.GetProgressInRelationToAverage(hallwayPointToWalkTo, Brain.Position);
            Brain.SetSpeed(Brain.BaseSpeed + SettingsSystem.AISettings.VariationOfSpeedBasedOnPositionComparedToGroup.Evaluate(progressInRelationToGroup));

            return TaskStatus.Running;
        }

        private void UpdateHallwayProgress()
        {
            hallwayProgress.Destination = hallwayPointToWalkTo;
            hallwayProgress.Position = Brain.Position;
        }

        private void SeekNextPoint()
        {
            if (!HallwayManager.HasInstance)
                return;

            hallwayPointToWalkTo = hallwayToWalkIn.GetNextPoint(hallwayPointToWalkTo);
            positionToWalkTo = hallwayPointToWalkTo.GetRandomPosition();
        }
        
        private void InitHallwayToWalkIn()
        {
            if (!HallwayManager.HasInstance)
                return;

            hallwayToWalkIn = HallwayManager.Instance.GetRandomHallway();
            if (!hallwayToWalkIn)
                return;
            
            hallwayToWalkIn.JoinGroup(hallwayProgress);                
            hallwayPointToWalkTo = hallwayToWalkIn.GetClosestPointTo(Brain.Position);
            positionToWalkTo = hallwayPointToWalkTo.GetRandomPosition();
        }

        public override void OnEnd()
        {
            if (hallwayToWalkIn)
                hallwayToWalkIn.LeaveGroup(hallwayProgress);
            
            hallwayToWalkIn = null;
            hallwayPointToWalkTo = null;
            hallwayProgress = null;
            
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