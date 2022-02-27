﻿using BehaviorDesigner.Runtime.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Units.AI.Actions
{
    [TaskCategory("AI")]
    public abstract class AIAction : Action
    {
        [SerializeField, Required] private AIBrain brain = null;
        
        protected AIBrain Brain => brain;
        
        public override void OnStart()
        {
            base.OnStart();
            Debug.Assert(brain, $"A brain should be assigned to AI action (missing one in object {gameObject.name}-{FriendlyName})");
        }

        public override void OnReset()
        {
            base.OnReset();
            brain = null;
        }
    }
}