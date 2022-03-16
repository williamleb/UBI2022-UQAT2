using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Interfaces;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities.Extensions;

namespace Managers.Hallway
{
    public class Hallway : NetworkBehaviour, IProbabilityObject
    {
        [SerializeField, ValidateInput(nameof(ValidateHallwayPoints), "There must be at least one hallway point")] private List<HallwayPoint> hallwayPoints = new List<HallwayPoint>();
        [SerializeField] private HallwayColor color = HallwayColor.White;
        [SerializeField, PropertyRange(0.01f, 1f)] private float probability = 0.5f;

        private readonly List<HallwayProgress> hallwayGroup = new List<HallwayProgress>();
        private float groupAverageProgress = 0f;
        
        public float Probability => probability;
        public int HallwayId => Id.GetHashCode();
        public int Size => hallwayPoints.Count;
        public HallwayColor Color => color;

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

        // Returns a value between -0.5 and 0.5
        public float GetProgressInRelationToAverage(HallwayPoint destination, Vector3 currentPosition)
        {
            var progress = GetProgress(destination, currentPosition);

            float forwardProgressRelation;
            float backwardProgressRelation;
            if (groupAverageProgress < progress)
            {
                forwardProgressRelation = 1f - progress + groupAverageProgress;
                backwardProgressRelation = progress - groupAverageProgress;
            }
            else
            {
                forwardProgressRelation = groupAverageProgress - progress;
                backwardProgressRelation = 1 - groupAverageProgress + progress;
            }
            
            return forwardProgressRelation < backwardProgressRelation ? -backwardProgressRelation : forwardProgressRelation;
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

#if UNITY_EDITOR
        [Button("BuildHallwayFromChildObjects")]
        private void BuildHallwayFromChildObjects()
        {
            AddChildrenAsHallwayPoints();
            AlignHallwayPointWithPath();
        }

        private void AddChildrenAsHallwayPoints()
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

        private void AlignHallwayPointWithPath()
        {
            foreach (var currentPoint in hallwayPoints)
            {
                var currentPosition = currentPoint.transform.position;
                var previousPointPosition = GetPreviousPoint(currentPoint).transform.position;
                var nextPointPosition = GetNextPoint(currentPoint).transform.position;

                var previousDirection = (currentPosition - previousPointPosition).normalized;
                var nextDirection = (nextPointPosition - currentPosition).normalized;

                var newForward = (previousDirection + nextDirection).normalized;

                currentPoint.transform.forward = newForward;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (hallwayPoints.Count < 2)
                return;

            foreach (var point in hallwayPoints)
            {
                point.GizmoColor = GetGizmoColor();
            }

            Gizmos.color = GetGizmoColor(); 
            for (var i = 0; i < hallwayPoints.Count; ++i)
            {
                Gizmos.DrawLine(
                    hallwayPoints[i].transform.position + Vector3.up, 
                    hallwayPoints[(i+1) % hallwayPoints.Count].transform.position + Vector3.up);
            }
        }

        private Color GetGizmoColor()
        {
            switch (color)
            {
                case HallwayColor.White:
                    return UnityEngine.Color.white;
                case HallwayColor.Red:
                    return new Color32(0xff, 0xab, 0x91, 0xff);
                case HallwayColor.Green:
                    return new Color32(0xc5, 0xe1, 0xa5, 0xff);
                case HallwayColor.Blue:
                    return new Color32(0x81, 0xd4, 0xfa, 0xff);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private bool ValidateHallwayPoints()
        {
            return hallwayPoints.Count > 1;
        }
#endif
    }
}