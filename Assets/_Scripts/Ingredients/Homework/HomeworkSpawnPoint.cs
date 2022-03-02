using Sirenix.OdinInspector;
using UnityEngine;

namespace Ingredients.Homework
{
    public class HomeworkSpawnPoint : MonoBehaviour
    {
        [Tooltip("Relative probability of this particular spawner to be chosen compared to others. If all spawners have a value of 0.5 for this field, they will all have the same probability of being chosen.")]
        [SerializeField, PropertyRange(0.01f, 1f)] private float probability = 0.5f;

        public float Probability => probability;
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
        
            var position = transform.position;
            Gizmos.DrawSphere(position, 0.5f);
            Gizmos.DrawIcon(position + Vector3.up * 2f, "d_Project@2x", true, Color.yellow);
        }
    }
}