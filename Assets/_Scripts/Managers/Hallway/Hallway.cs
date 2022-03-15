using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Interfaces;
using Sirenix.OdinInspector;
using Units.AI;
using UnityEngine;
using Utilities.Extensions;

namespace Managers.Hallway
{
    public class Hallway : NetworkBehaviour, IProbabilityObject
    {
        [SerializeField, PropertyRange(0.01f, 1f)] private float probability = 0.5f;
        [SerializeField, ValidateInput(nameof(ValidateHallwayPoints), "There must be at least one hallway point")] private List<HallwayPoint> hallwayPoints = new List<HallwayPoint>();

        private readonly List<HallwayProgress> hallwayGroup = new List<HallwayProgress>();
        private float groupAverageProgress = 0f;

        public float Probability => probability;
        public int HallwayId => Id.GetHashCode();
        public int Size => hallwayPoints.Count;

        public void JoinGroup(HallwayProgress entity)
        {
            hallwayGroup.Add(entity);
        }

        public void LeaveGroup(HallwayProgress entity)
        {
            hallwayGroup.Remove(entity);
        }

        private void UpdateGroupAverageProgress()
        {
            if (!hallwayGroup.Any())
                return;
            
            var averageX = 0f;
            var averageY = 0f;
            foreach (var entity in hallwayGroup)
            {
                var circularProgress = GetProgress(entity.Destination, entity.Position) * 2 * Mathf.PI;
                averageX += Mathf.Cos(circularProgress);
                averageY += Mathf.Sin(circularProgress);
            }

            averageX /= hallwayGroup.Count;
            averageY /= hallwayGroup.Count;

            var averageCircularProgress = Mathf.Atan2(averageY, averageX);
            if (averageCircularProgress < 0f)
                averageCircularProgress += 2 * Mathf.PI;
            
            groupAverageProgress = averageCircularProgress / (2 * Mathf.PI);
        }

        private void Update()
        {
            UpdateGroupAverageProgress();
        }

        public HallwayPoint GetClosestPointTo(Vector3 position)
        {
            if (!hallwayPoints.Any())
                return null;
            
            var closestPoint = hallwayPoints.First();
            var minDistance = float.MaxValue;
            
            for (var i = 0; i < hallwayPoints.Count; ++i)
            {
                var distance = position.SqrDistanceWith(hallwayPoints[i].transform.position);
                if (distance < minDistance)
                {
                    closestPoint = hallwayPoints[i];
                    minDistance = distance;
                }
            }

            return closestPoint;
        }

        public HallwayPoint GetNextPoint(HallwayPoint point)
        {
            var indexFound = GetIndexOfPoint(point);
            var newIndex = (indexFound + 1) % hallwayPoints.Count;
            return hallwayPoints[newIndex];
        }
        
        public HallwayPoint GetPreviousPoint(HallwayPoint point)
        {
            var indexFound = GetIndexOfPoint(point);
            var newIndex = (indexFound + hallwayPoints.Count - 1) % hallwayPoints.Count;
            return hallwayPoints[newIndex];
        }
        
        public float GetProgress(HallwayPoint destination, Vector3 currentPosition)
        {
            var index = GetIndexOfPoint(destination);
            if (index == -1)
                return 0f;
            
            var passedIndex = (index + hallwayPoints.Count - 1) % hallwayPoints.Count;
            var indexProgress = passedIndex / (float) hallwayPoints.Count;

            var distanceToPassedPoint = currentPosition.SqrDistanceWith(hallwayPoints[passedIndex].transform.position);
            var distanceToDestinationPoint = currentPosition.SqrDistanceWith(destination.transform.position);
            var interIndexProgression = distanceToPassedPoint / (distanceToDestinationPoint + distanceToPassedPoint);
            interIndexProgression *= 1f / hallwayPoints.Count;

            return indexProgress + interIndexProgression;
        }

        public Vector3 GetPointForProgress(float progress)
        {
            if (progress < 0f)
                return hallwayPoints.First().transform.position;
            if (progress > 1f)
                return hallwayPoints.Last().transform.position;

            var progressForIndex = 1f / hallwayPoints.Count;
            var index = (int)(progress / progressForIndex);
            var progressForInterIndex = progress - (index * progressForIndex);
            progressForInterIndex *= hallwayPoints.Count;
            var nextIndex = (index + 1) % hallwayPoints.Count;

            return (1 - progressForInterIndex) * hallwayPoints[index].transform.position +
                   progressForInterIndex * hallwayPoints[nextIndex].transform.position;
        }

        public int GetIndexOfPoint(HallwayPoint point)
        {
            var indexFound = -1;
            for (var i = 0; i < hallwayPoints.Count; ++i)
            {
                if (hallwayPoints[i] == point)
                {
                    indexFound = i;
                    break;
                }
            }

            return indexFound;
        }
        
        public override void Spawned()
        {
            if (HallwayManager.HasInstance)
                HallwayManager.Instance.RegisterHallway(this);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (HallwayManager.HasInstance)
                HallwayManager.Instance.UnregisterHallway(this);
        }

        [Button("BuildHallwayFromChildObjects")]
        private void BuildHallwayFromChildObjects()
        {
            hallwayPoints.Clear();
                
            foreach (Transform child in transform)
            {
                var hallwayPoint = child.GetComponent<HallwayPoint>();
                if (!hallwayPoint)
                    continue;
                
                hallwayPoints.Add(hallwayPoint);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (hallwayPoints.Count < 2)
                return;

            Gizmos.color = Color.white;
            for (var i = 0; i < hallwayPoints.Count; ++i)
            {
                Gizmos.DrawLine(
                    hallwayPoints[i].transform.position + Vector3.up, 
                    hallwayPoints[(i+1) % hallwayPoints.Count].transform.position + Vector3.up);
            }
        }

        private bool ValidateHallwayPoints()
        {
            return hallwayPoints.Count > 1;
        }

        // TODO Remove
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(GetPointForProgress(groupAverageProgress) + Vector3.up, 0.5f);

            foreach (var entity in hallwayGroup)
            {
                var progress = GetProgress(entity.Destination, entity.Position);
                Gizmos.DrawSphere(GetPointForProgress(progress) + Vector3.up, 0.2f);
                
            }
        }
    }
}