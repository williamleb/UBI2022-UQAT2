using System;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;
using Utilities.Singleton;

namespace Systems
{
    public class RumbleSystem : PersistentSingleton<RumbleSystem>
    {
        private readonly Dictionary<Guid, RumbleContribution> contributions = new Dictionary<Guid, RumbleContribution>();

        public bool Activated { get; set; } = true;

        public RumbleKey GenerateNewRumbleKeyFromBehaviour(NetworkBehaviour networkBehaviour)
        {
            return new RumbleKey(networkBehaviour);
        }

        public void SetRumble(RumbleKey key, float lowFrequency)
        {
            SetRumble(key, lowFrequency);
        }

        public void SetRumble(RumbleKey key, float lowFrequency, float highFrequency)
        {
            Debug.Assert(lowFrequency >= 0f);
            Debug.Assert(highFrequency >= 0f);
            
            if (key == null || !key.CanInfluenceRumble)
                return;

            var contribution = new RumbleContribution()
            {
                LowFrequencyContribution = lowFrequency,
                HighFrequencyContribution = highFrequency
            };
            
            if (contributions.ContainsKey(key.Id))
            {
                contributions[key.Id] = contribution;
            }
            else
            {
                contributions.Add(key.Id, contribution);
            }
            
            UpdateRumble();
        }

        public void StopRumble(RumbleKey key)
        {
            if (key == null)
                return;
            
            if (contributions.ContainsKey(key.Id))
            {
                contributions.Remove(key.Id);
            }

            UpdateRumble();
        }

        public void StopAllRumble()
        {
            contributions.Clear();
            UpdateRumble();
        }

        private void UpdateRumble()
        {
            if (Gamepad.current == null)
                return;
            
            if (contributions.Count == 0 || !Activated)
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
    
    public class RumbleKey
    {
        public bool CanInfluenceRumble { get; private set; }
        public Guid Id { get; private set; }

        public RumbleKey(SimulationBehaviour behaviour)
        {
            CanInfluenceRumble = behaviour.Object.HasInputAuthority;
            Id = Guid.NewGuid();
        }
    }
}