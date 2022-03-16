using System;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Units.AI;
using Units.AI.Actions;
using Units.Player;
using UnityEngine;
using Utilities.Extensions;

namespace _Scripts.Units.AI.Actions
{
    [Serializable]
    [TaskCategory("AI")]
    [TaskDescription("Hit an entity.")]
    public class Hit : AIAction
    {
        [SerializeField] private SharedTransform entityToHitTransform = null;
        [SerializeField] private SharedBool overrideImmobilizationTime = false;
        [SerializeField] private SharedFloat immobilizationTime = 0f;

        public override TaskStatus OnUpdate()
        {
            if (!entityToHitTransform?.Value)
                return TaskStatus.Failure;

            var entityGameObject = entityToHitTransform.Value.gameObject;
            if (entityGameObject.IsAPlayer())
                HitPlayer(entityGameObject);
            else if (entityGameObject.IsAnAI())
                HitAI(entityGameObject);
            else
                return TaskStatus.Failure;

            return TaskStatus.Success;
        }

        private void HitPlayer(GameObject playerGameObject)
        {
            var playerEntity = playerGameObject.GetComponentInEntity<PlayerEntity>();
            Debug.Assert(playerEntity);
            
            if (overrideImmobilizationTime.Value)
                playerEntity.ExternalHit(immobilizationTime.Value);
            else
                playerEntity.ExternalHit();
        }

        private void HitAI(GameObject aiGameObject)
        {
            var aiEntity = aiGameObject.GetComponentInEntity<AIEntity>();
            Debug.Assert(aiEntity);
            
            if (overrideImmobilizationTime.Value)
                aiEntity.Hit(Brain.Entity.gameObject, immobilizationTime.Value);
            else
                aiEntity.Hit(Brain.Entity.gameObject);
        }

        public override void OnReset()
        {
            base.OnReset();
            entityToHitTransform = null;
            overrideImmobilizationTime = false;
            immobilizationTime = 0f;
        }
    }
}