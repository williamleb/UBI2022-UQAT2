using System;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Managers.Score;
using Units.Player;
using UnityEngine;
using Utilities.Extensions;
using Action = BehaviorDesigner.Runtime.Tasks.Action;

namespace Units.AI.Actions
{
    [Serializable]
    [TaskCategory("Score")]
    public class MakePlayerLoseScore : Action
    {
        [SerializeField] private SharedTransform targetPlayer = null;
        [SerializeField] private SharedInt scoreToRemove = 1;

        public override TaskStatus OnUpdate()
        {
            if (!targetPlayer.Value)
                return TaskStatus.Failure;

            if (!targetPlayer.Value.gameObject.IsAPlayer())
                return TaskStatus.Failure;

            var player = targetPlayer.Value.gameObject.GetComponentInEntity<PlayerEntity>();
            if (!player)
                return TaskStatus.Failure;

            if (!ScoreManager.HasInstance)
                return TaskStatus.Success;
            
            ScoreManager.Instance.DecrementScore(player, scoreToRemove.Value);
            return TaskStatus.Success;
        }

        public override void OnReset()
        {
            base.OnReset();
            targetPlayer = null;
            scoreToRemove = 1;
        }
    }
}