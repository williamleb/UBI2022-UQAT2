using System;
using System.Collections.Generic;
using Fusion;
using Managers.Interactions;
using Sirenix.OdinInspector;
using Units.Player;
using UnityEngine;
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

        private readonly List<PlayerEntity> playersInSight = new List<PlayerEntity>();
        private readonly List<AIEntity> aisInSight = new List<AIEntity>();
        private readonly List<Interaction> interactionsInSight = new List<Interaction>();

        public IEnumerable<PlayerEntity> PlayersInSight => playersInSight;
        public IEnumerable<AIEntity> AIsInSight => aisInSight;
        public IEnumerable<Interaction> InteractionsInSight => interactionsInSight;

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

        private void DetectObjectsInVision()
        {
            playersInSight.Clear();
            aisInSight.Clear();
            interactionsInSight.Clear();
            
            var colliders = new Collider[10];
            var thisTransform = transform;
            var halfExtents = new Vector3(farLength / 2f, 10f, far);
            
            if (Runner.GetPhysicsScene().OverlapBox(thisTransform.position, halfExtents, colliders, thisTransform.rotation, Physics.AllLayers) <= 0) return;

            foreach (var objectCollider in colliders)
            {
                if (!objectCollider)
                    continue;
                
                ManageCollider(objectCollider);
            }
        }

        private void ManageCollider(Collider objectCollider)
        {
            if (!IsInFrustum(objectCollider.transform.position))
                return;

            if (objectCollider.CompareTag(PlayerEntity.TAG))
                ManagePlayerCollider(objectCollider);
            else if (objectCollider.CompareTag(AIEntity.TAG))
                ManageAICollider(objectCollider);
            else if (objectCollider.CompareTag(Interaction.TAG))
                ManageInteractionCollider(objectCollider);
        }

        private void ManagePlayerCollider(Collider playerCollider)
        {
            if (!IsVisible(playerCollider))
                return;

            var playerEntity = playerCollider.gameObject.GetComponentInEntity<PlayerEntity>();
            if (!playerEntity)
                return;
            
            playersInSight.Add(playerEntity);
        }
        
        private void ManageAICollider(Collider aiCollider)
        {
            if (!IsVisible(aiCollider))
                return;

            var aiEntity = aiCollider.gameObject.GetComponentInEntity<AIEntity>();
            if (!aiEntity)
                return;
            
            aisInSight.Add(aiEntity);
        }
        
        private void ManageInteractionCollider(Collider interactionCollider)
        {
            if (!IsVisible(interactionCollider))
                return;

            var interaction = interactionCollider.gameObject.GetComponentInEntity<Interaction>();
            if (!interaction)
                return;
            
            interactionsInSight.Add(interaction);
        }

        private RaycastHit hit;
        private bool IsVisible(Collider objectCollider)
        {
            if (!Physics.Raycast(transform.position, objectCollider.transform.position - transform.position, out hit))
                return false;

            if (!objectCollider.gameObject.CompareEntities(hit.collider.gameObject))
                return false;

            return true;
        }

        private bool IsInFrustum(Vector3 position)
        {
            var thisTransform = transform;
            var direction = position - thisTransform.position;
            if (Vector3.Dot(direction, transform.forward) < 0)
                return false;
            
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
            DrawSightFrustumGizmo();
            DrawObjectsInSightGizmos();
        }

        private void DrawSightFrustumGizmo()
        {
            var frustumMesh = CreateFrustumMesh();
            var thisTransform = transform;

            Gizmos.color = Color.green;
            Gizmos.DrawMesh(frustumMesh, thisTransform.position, thisTransform.rotation);
        }

        private void DrawObjectsInSightGizmos()
        {
            Gizmos.color = Color.yellow;
            foreach (var interaction in interactionsInSight)
            {
                Gizmos.DrawSphere(interaction.transform.position + Vector3.up * 2f, 0.5f);
            }
            
            Gizmos.color = Color.cyan;
            foreach (var player in playersInSight)
            {
                Gizmos.DrawSphere(player.transform.position + Vector3.up * 2f, 0.5f);
            }
            
            Gizmos.color = Color.magenta;
            foreach (var ai in aisInSight)
            {
                Gizmos.DrawSphere(ai.transform.position + Vector3.up * 2f, 0.5f);
            }
        }
#endif
    }
}