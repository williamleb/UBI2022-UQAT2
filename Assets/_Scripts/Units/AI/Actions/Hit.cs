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
            playerEntity.ExternalHit();
        }

        private void HitAI(GameObject aiGameObject)
        {
            var aiEntity = aiGameObject.GetComponentInEntity<AIEntity>();
            Debug.Assert(aiEntity);
            aiEntity.Hit();
        }

        public override void OnReset()
        {
            base.OnReset();
            entityToHitTransform = null;
        }
    }
}