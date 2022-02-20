using System;
using UnityEngine;

namespace Ingredients.Homework
{
    public class HomeworkSpawnPoint : MonoBehaviour
    {
        private void OnDrawGizmosSelected()
        {
            
            var thisTransform = transform;
            var position = thisTransform.position;
            var up = thisTransform.up;
            var right = thisTransform.right;
            var forward = thisTransform.forward;
            
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(position, 0.25f);

            Gizmos.DrawLine(up * 10f + position, -up * 10f + position);
            Gizmos.DrawLine(right * 10f + position, -right * 10f + position);
            Gizmos.DrawLine(forward * 10f + position, -forward * 10f + position);
        }
    }
}