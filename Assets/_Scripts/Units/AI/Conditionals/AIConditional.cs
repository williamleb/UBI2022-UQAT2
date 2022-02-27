﻿using BehaviorDesigner.Runtime.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Units.AI.Conditionals
{
    [TaskCategory("AI")]
    public abstract class AIConditional : Conditional
    {
        [SerializeField, Required] private AIBrain brain = null;
        
        protected AIBrain Brain => brain;

        public override void OnStart()
        {
            base.OnStart();
            Debug.Assert(brain, $"A brain should be assigned to AI conditional (missing one in object {gameObject.name}-{FriendlyName})");
        }
        
        public override void OnReset()
        {
            base.OnReset();
            brain = null;
        }
    }
}