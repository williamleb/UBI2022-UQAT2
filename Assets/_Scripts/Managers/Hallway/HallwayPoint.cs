using UnityEngine;

namespace Managers.Hallway
{
    public class HallwayPoint : MonoBehaviour
    {
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.white;
        
            var position = transform.position;
            Gizmos.DrawSphere(position, 0.5f);
            Gizmos.DrawIcon(position + Vector3.up * 2f, "d_Record Off@2x", true, Color.white);
        }
    }
}