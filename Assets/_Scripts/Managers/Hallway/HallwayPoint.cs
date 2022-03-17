using System;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Managers.Hallway
{
    public class HallwayPoint : MonoBehaviour
    {
        [SerializeField, MinValue(0f)] private float width = 4f;

        private Vector3 position;
        private Vector3 forward;
        private Vector3 right;

        private void Awake()
        {
            var thisTransform = transform;
            position = thisTransform.position;
            forward = thisTransform.forward;
            right = thisTransform.right;
        }

        public Vector3 GetRandomPosition()
        {
            var randomWidth = Random.Range(0f, 1f);
            return position + right * width * (-0.5f + randomWidth);
        }
        
        public bool HasPassedThisPoint(Vector3 objectPosition)
        {
            return Vector3.Dot(position - objectPosition, forward) < 0;
        }
        
#if UNITY_EDITOR
        public Color GizmoColor { get; set; } = Color.white;
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = GizmoColor;

            var thisTransform = transform;
            var gizmoPosition = thisTransform.position;
            var gizmoForward = thisTransform.forward;
            var gizmoRight = thisTransform.right;
            
            Gizmos.DrawSphere(gizmoPosition, 0.5f);
            Gizmos.DrawSphere(gizmoPosition + gizmoForward * 0.5f, 0.2f);
            Gizmos.DrawSphere(gizmoPosition + gizmoForward * 1.0f, 0.2f);
            Gizmos.DrawSphere(gizmoPosition + gizmoForward * 1.5f, 0.2f); 
            
            Gizmos.DrawLine(gizmoPosition + Vector3.up, gizmoPosition + Vector3.up + gizmoRight * width / 2f);
            Gizmos.DrawLine(gizmoPosition + Vector3.up, gizmoPosition + Vector3.up - gizmoRight * width / 2f); 
            
            Gizmos.DrawIcon(gizmoPosition + Vector3.up * 2f, "d_Record Off@2x", true, Color.white);
        }
#endif
    }
}