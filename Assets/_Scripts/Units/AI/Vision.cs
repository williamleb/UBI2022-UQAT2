using System;
using System.Collections.Generic;
using Fusion;
using Managers.Interactions;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Units.Player;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using Utilities.Extensions;

namespace Units.AI
{
    public class Vision : NetworkBehaviour
    {
        [SerializeField, MinValue(0.1f)] private float near = 1f;
        [SerializeField, MinValue(0.2f)] private float far = 10f;
        [SerializeField, MinValue(0.1f)] private float nearLength = 2f;
        [SerializeField, MinValue(0.2f)] private float farLength = 20f;

        private Matrix4x4 verificationMatrix;

        private List<PlayerEntity> playersInSight = new List<PlayerEntity>();
        private List<AIEntity> aisInSight = new List<AIEntity>();
        private List<Interaction> interactionsInSight = new List<Interaction>();

        private List<GameObject> lol = new List<GameObject>();

        private void Start()
        {
            ComputeVerificationMatrix();
        }

        public override void Spawned()
        {
            InvokeRepeating(nameof(DetectObjectsInVision),1,0.1f);
        }

        private void ComputeVerificationMatrix()
        {
            // Matrix to transform a position in order to easily know if it is in the vision frustum
            // n: near, f: far, d: nearLength, l: farLength
            // | (2*n)/2*d      0      0        0      |
            // |      0    (f+n)/(n-f) 0 (2*f*n)/(f-n) |
            // |      0         0      1        0      |
            // |      0    (l/d)/(f-n) 0        0      |
            verificationMatrix = new Matrix4x4(
                new Vector4((2 * near) / nearLength, 0, 0, 0),
                new Vector4(0, (far + near)/(near - far), 0, (farLength / nearLength) / (far - near)),
                new Vector4(0, 0, 1, 0),
                new Vector4(0, (2 * far * near)/(far - near), 0, 0));
        }

        private int counter = 0;
        private void DetectObjectsInVision()
        {
            playersInSight.Clear();
            aisInSight.Clear();
            interactionsInSight.Clear();
            
            lol.Clear();
            
            var colliders = new Collider[10];
            var thisTransform = transform;
            var halfExtents = new Vector3(farLength / 2f, 10f, far);
            
            if (Runner.GetPhysicsScene().OverlapBox(thisTransform.position, halfExtents, colliders, thisTransform.rotation, Physics.AllLayers) <= 0) return;

            foreach (var objectCollider in colliders)
            {
                if (!objectCollider)
                    continue;
                
                if (objectCollider.CompareTag(PlayerEntity.TAG))
                {
                    Debug.Log($"In {counter}");
                    ++counter;
                }
                
                if (!IsInFrustum(objectCollider.transform.position))
                    continue;

                if (objectCollider.CompareTag(PlayerEntity.TAG))
                {
                    lol.Add(objectCollider.gameObject);
                }
                else if (objectCollider.CompareTag(AIEntity.TAG))
                {
                    lol.Add(objectCollider.gameObject);
                }
                else if (objectCollider.CompareTag(Interaction.TAG))
                {
                    lol.Add(objectCollider.gameObject);
                }
            }
        }

        private bool IsInFrustum(Vector3 position)
        {
            var thisTransform = transform;
            var direction = position - thisTransform.position;
            var rotatedDirection = Quaternion.Euler(0f, -thisTransform.eulerAngles.y, 0f) * direction;
            var localPosition = new Vector4(rotatedDirection.x, rotatedDirection.z, 1, 1);

            var normalizedPosition = verificationMatrix * localPosition;
            normalizedPosition /= normalizedPosition.w;

            return normalizedPosition.x >= -1f && normalizedPosition.x <= 1f && 
                   normalizedPosition.y >= -1f && normalizedPosition.y <= 1f;
        }

        private void OnValidate()
        {
            far = Math.Max(near + 0.1f, far);
            farLength = Math.Max(nearLength + 0.1f, farLength);
            
            if (Application.isPlaying)
            {
                ComputeVerificationMatrix();
            }
        }

#if UNITY_EDITOR
        private Mesh CreateFrustumMesh()
        {
            var position = transform.position;
            var upperLeftCorner = new Vector3(-farLength / 2f, position.y, far);
            var upperRightCorner = new Vector3(farLength / 2f, position.y, far);
            var lowerRightCorner = new Vector3(nearLength / 2f, position.y, near);
            var lowerLeftCorner = new Vector3(-nearLength / 2f, position.y, near);
            
            return new Mesh
            {
                vertices = new[] {upperLeftCorner, upperRightCorner, lowerRightCorner, lowerLeftCorner},
                normals = new[] {Vector3.up, Vector3.up, Vector3.up, Vector3.up},
                uv = new [] {new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1)},
                triangles = new[] {0, 1, 3, 2, 3, 1}
            };
        }
        
        private void OnDrawGizmosSelected()
        {
            var frustumMesh = CreateFrustumMesh();
            var thisTransform = transform;

            Gizmos.color = Color.green;
            Gizmos.DrawMesh(frustumMesh, thisTransform.position, thisTransform.rotation);

            Gizmos.color = Color.red;
            foreach (var ok in lol)
            {
                Gizmos.DrawSphere(ok.transform.position + Vector3.up * 2f, 0.5f);
            }
            
            
            var position = transform.position;
            var upperLeftCorner = new Vector3(-farLength / 2f, position.y, far);
            var upperRightCorner = new Vector3(farLength / 2f, position.y, far);
            var lowerRightCorner = new Vector3(nearLength / 2f, position.y, near);
            var lowerLeftCorner = new Vector3(-nearLength / 2f, position.y, near);
            
            DoIt(upperLeftCorner + position, Color.blue);
            DoIt(upperRightCorner + position, Color.blue);
            DoIt(lowerRightCorner + position, Color.blue);
            DoIt(lowerLeftCorner + position, Color.blue);

            foreach (var ok in lol)
            {
                DoIt(ok.transform.position, Color.yellow);
            }
            
            DoIt(transform.position, Color.cyan);

            var salut = transform.rotation * (transform.position + new Vector3(0f, 0f, (far + near) / 2f));
            Gizmos.color = Color.black;
            Gizmos.DrawSphere(salut, 1f);
            
            var rotation = thisTransform.rotation;
            var boxCenter = transform.position;
            var halfExtents = new Vector3(farLength / 2f, 10f, far);
            halfExtents = halfExtents.Abs();
            
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(boxCenter + new Vector3(halfExtents.x, 0, halfExtents.z), 0.2f);
            Gizmos.DrawSphere(boxCenter + new Vector3(-halfExtents.x, 0, halfExtents.z), 0.2f);
            Gizmos.DrawSphere(boxCenter + new Vector3(halfExtents.x, 0, -halfExtents.z), 0.2f);
            Gizmos.DrawSphere(boxCenter + new Vector3(-halfExtents.x, 0, -halfExtents.z), 0.2f);
        }


        private void DoIt(Vector3 position, Color color)
        {
            var thisTransform = transform;
            var direction = position - thisTransform.position;
            var rotatedDirection = Quaternion.Euler(0f, -thisTransform.eulerAngles.y, 0f) * direction;
            var localPosition = new Vector4(rotatedDirection.x, rotatedDirection.z, 1, 1);

            var normalizedPosition = verificationMatrix * localPosition;
            normalizedPosition /= normalizedPosition.w;

            Gizmos.color = color;
            Gizmos.DrawSphere(new Vector3(normalizedPosition.x * 10f, 1f, normalizedPosition.y * 10f), 0.5f);
        }
#endif
    }
}