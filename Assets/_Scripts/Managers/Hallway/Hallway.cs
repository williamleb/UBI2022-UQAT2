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
        [SerializeField, PropertyRange(0.01f, 1f)] private float probability = 0.5f;
        [SerializeField, ValidateInput(nameof(ValidateHallwayPoints), "There must be at least one hallway point")] private List<HallwayPoint> hallwayPoints = new List<HallwayPoint>();

        private bool advanceToNextPoint = false;
        
        public float Probability => probability;
        public int HallwayId => Id.GetHashCode();

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

        public HallwayPoint GetNextPoint(HallwayPoint previousPoint)
        {
            var indexFound = -1;
            for (var i = 0; i < hallwayPoints.Count; ++i)
            {
                if (hallwayPoints[i] == previousPoint)
                {
                    indexFound = i;
                    break;
                }
            }

            var newIndex = (indexFound + 1) % hallwayPoints.Count;
            return hallwayPoints[newIndex];
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
    }
}