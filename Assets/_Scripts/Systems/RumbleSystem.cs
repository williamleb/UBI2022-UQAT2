using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Utilities.Singleton;

namespace Systems
{
    public class RumbleSystem : PersistentSingleton<RumbleSystem>
    {
        private readonly Dictionary<object, RumbleContribution> contributions = new Dictionary<object, RumbleContribution>();

        public void SetRumble(object key, float lowFrequency)
        {
            SetRumble(key, lowFrequency);
        }
        
        public void SetRumble(object key, float lowFrequency, float highFrequency)
        {
            Debug.Assert(key != null);
            Debug.Assert(lowFrequency >= 0f);
            Debug.Assert(highFrequency >= 0f);
            
            var contribution = new RumbleContribution()
            {
                LowFrequencyContribution = lowFrequency,
                HighFrequencyContribution = highFrequency
            };
            
            if (contributions.ContainsKey(key))
            {
                contributions[key] = contribution;
            }
            else
            {
                contributions.Add(key, contribution);
            }
            
            UpdateRumble();
        }

        public void StopRumble(object key)
        {
            if (contributions.ContainsKey(key))
            {
                contributions.Remove(key);
            }

            UpdateRumble();
        }

        private void UpdateRumble()
        {
            if (contributions.Count == 0)
            {
                Gamepad.current.SetMotorSpeeds(0.0f, 0.0f);
                return;
            }

            var sumOfContributions = new RumbleContribution();
            foreach (var contribution in contributions.Values)
            {
                sumOfContributions += contribution;
            }
            
            Gamepad.current.SetMotorSpeeds(
                Math.Min(sumOfContributions.LowFrequencyContribution, 1f), 
                Math.Min(sumOfContributions.HighFrequencyContribution, 1f));
        }
        
        private struct RumbleContribution
        {
            public float LowFrequencyContribution;
            public float HighFrequencyContribution;
            
            public static RumbleContribution operator+(RumbleContribution left, RumbleContribution right)
            {
                return new RumbleContribution()
                {
                    LowFrequencyContribution = left.LowFrequencyContribution + right.LowFrequencyContribution,
                    HighFrequencyContribution = left.HighFrequencyContribution + right.HighFrequencyContribution
                };
            }
        }
    }
}