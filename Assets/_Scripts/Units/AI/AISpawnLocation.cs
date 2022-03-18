using System;
using Fusion;
using Managers.Hallway;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Units.AI
{
    public class AISpawnLocation : MonoBehaviour
    {
        [SerializeField, Required] private NetworkObject aiEntityPrefab;
        [SerializeField, Required] private GameObject aiBrainPrefab;
        [SerializeField] private HallwayColor assignedHallway;

        [Header("Editor Only")]
        [SerializeField] private Color gizmoColor;

        public NetworkObject AIEntityPrefab => aiEntityPrefab;
        public GameObject AIBrainPrefab => aiBrainPrefab;
        public HallwayColor AssignedHallway => assignedHallway;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = gizmoColor;
            
            var position = transform.position;
            Gizmos.DrawSphere(position, 0.5f);
            Gizmos.DrawIcon(position + Vector3.up * 2f, "BuildSettings.Android", true, gizmoColor);
        }
    }
}