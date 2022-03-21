using UnityEngine;

namespace Units.Player
{
    public class PlayerSpawnLocation : MonoBehaviour
    {
        [Header("Editor Only")]
        [SerializeField] private Color gizmoColor;
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = gizmoColor;
            
            var position = transform.position;
            Gizmos.DrawSphere(position, 0.5f);
            Gizmos.DrawIcon(position + Vector3.up * 2f, "BuildSettings.Android", true, gizmoColor);
        }
    }
}