using System;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Managers.Game;
using UnityEngine;

namespace Units.AI.Conditionals
{
    [Serializable]
    [TaskDescription("Returns success if the game state matches the chosen game state.")]
    [TaskCategory("Game")]
    public class IsGameState : Conditional
    {
        [SerializeField] private GameState gameState = GameState.Running;
        [SerializeField] private SharedBool failOnNoGameManagerInstance = true;

        public override TaskStatus OnUpdate()
        {
            if (!GameManager.HasInstance)
                return failOnNoGameManagerInstance.Value ? TaskStatus.Failure : TaskStatus.Success;

            return GameManager.Instance.CurrentState == gameState ? TaskStatus.Success : TaskStatus.Failure;
        }

        public override void OnReset()
        {
            base.OnReset();
            gameState = GameState.Running;
            failOnNoGameManagerInstance = true;
        }
    }
}