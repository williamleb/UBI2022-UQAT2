using System.Collections.Generic;
using Fusion;
using Managers.Interactions;
using Systems.Settings;
using Units.Player;
using UnityEngine;
using Utilities.Extensions;
using Utilities.Unity;

namespace Units.AI.Senses
{
    public class Vision : NetworkBehaviour
    {
        private readonly List<PlayerEntity> playersInSight = new List<PlayerEntity>();
        private readonly List<AIEntity> aisInSight = new List<AIEntity>();
        private readonly List<Interaction> interactionsInSight = new List<Interaction>();

        private AISettings data;

        public IEnumerable<PlayerEntity> PlayersInSight => playersInSight;
        public IEnumerable<AIEntity> AIsInSight => aisInSight;
        public IEnumerable<Interaction> InteractionsInSight => interactionsInSight;

        private void Awake()
        {
            data = SettingsSystem.AISettings;
        }

        public override void Spawned()
        {
            InvokeRepeating(nameof(DetectObjectsInVision),1,0.1f);
        }

        private readonly Collider[] colliders = new Collider[10];
        private void DetectObjectsInVision()
        {
            playersInSight.Clear();
            aisInSight.Clear();
            interactionsInSight.Clear();

            if (Runner && Runner.GetPhysicsScene().OverlapSphere(transform.position, data.VisionMaxDistance, colliders,Layers.GAMEPLAY_MASK, QueryTriggerInteraction.UseGlobal) <= 0) return;

            foreach (var objectCollider in colliders)
            {
                if (!objectCollider)
                    return;
                
                if (!IsDetected(objectCollider)) continue;
                if (!IsVisible(objectCollider)) continue;
                ManageCollider(objectCollider);
            }
        }

        private bool IsDetected(Collider other)
        {
            return IsInViewAngle(other) || IsInInstantDetectRange(other);
        }
        
        private bool IsInViewAngle(Collider other)
        {
            Vector3 dirToOther = (other.transform.position - transform.position).normalized;
            return Vector3.Angle(transform.forward,dirToOther) < data.VisionViewAngle;
        }

        private bool IsInInstantDetectRange(Collider other)
        {
            float distanceToOther = Vector3.Distance(transform.position, other.transform.position);
            return distanceToOther <= data.InstantDetectRange;
        }

        private void ManageCollider(Collider objectCollider)
        {
            if (objectCollider.CompareTag(Tags.PLAYER))
                ManagePlayerCollider(objectCollider);
            else if (objectCollider.CompareTag(Tags.AI))
                ManageAICollider(objectCollider);
            else if (objectCollider.CompareTag(Tags.INTERACTION))
                ManageInteractionCollider(objectCollider);
        }

        private void ManagePlayerCollider(Collider playerCollider)
        {
            var playerEntity = playerCollider.gameObject.GetComponentInEntity<PlayerEntity>();
            if (!playerEntity)
                return;
            
            playersInSight.Add(playerEntity);
        }
        
        private void ManageAICollider(Collider aiCollider)
        {
            var aiEntity = aiCollider.gameObject.GetComponentInEntity<AIEntity>();
            if (!aiEntity)
                return;
            
            aisInSight.Add(aiEntity);
        }
        
        private void ManageInteractionCollider(Collider interactionCollider)
        {
            var interaction = interactionCollider.gameObject.GetComponentInEntity<Interaction>();
            if (!interaction)
                return;
            
            interactionsInSight.Add(interaction);
        }

        private RaycastHit hit;
        private bool IsVisible(Collider objectCollider)
        {
            if (Runner == null || !Runner.GetPhysicsScene().Raycast(transform.position + Vector3.up, objectCollider.transform.position - transform.position, out hit))
                return false;

            if (!objectCollider.gameObject.CompareEntities(hit.collider.gameObject))
                return false;

            return true;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (Application.isPlaying)
            {
                Gizmos.color = Color.green;
                Vector3 pos = transform.position;
                Gizmos.DrawWireSphere(pos, data.VisionMaxDistance);
                float angleA = data.VisionViewAngle + transform.eulerAngles.y;
                float angleB = data.VisionViewAngle - transform.eulerAngles.y;
                Vector3 viewAngleA = new Vector3(Mathf.Sin(angleA * Mathf.Deg2Rad), 0, Mathf.Cos(angleA * Mathf.Deg2Rad));
                Vector3 viewAngleB = new Vector3(Mathf.Sin(-angleB * Mathf.Deg2Rad), 0, Mathf.Cos(-angleB * Mathf.Deg2Rad));

                Gizmos.DrawLine(pos, pos + viewAngleA * data.VisionMaxDistance);
                Gizmos.DrawLine(pos, pos + viewAngleB * data.VisionMaxDistance);
                
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(pos, data.InstantDetectRange);
            }
            DrawObjectsInSightGizmos();
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