using System;
using Fusion;
using Systems;
using Systems.Network;
using UnityEngine;

namespace Units.Player
{
    public partial class PlayerEntity
    {
        [Header("Throw")] 
        [SerializeField] private AnimationCurve lowFrequencyThrowRumbleCurve;
        [SerializeField] private AnimationCurve highFrequencyThrowRumbleCurve;

        private RumbleKey throwRumbleKey;
        private float throwForceTimer;
        
        [Networked] private NetworkBool IsAiming { get; set; } = false;
        [Networked] private NetworkBool IsThrowing { get; set; } = false;
        
        private float ThrowForcePercent => data.SecondsBeforeMaxThrowForce != 0 ? throwForceTimer / data.SecondsBeforeMaxThrowForce : 1f;

        private void InitThrow()
        {
            throwRumbleKey = RumbleSystem.Instance.GenerateNewRumbleKeyFromBehaviour(this);
        }
        
        private void ThrowUpdate(NetworkInputData inputData)
        {
            if (!CanThrow())
            {
                if (IsAiming)
                    CancelAiming();
                
                return;
            }

            if (inputData.IsThrow && !inMenu)
            {
                UpdateAim();
            }
            else
            {
                if (IsAiming)
                {
                    Throw();
                }
            }
        }

        private void UpdateAim()
        {
            if (!IsAiming) StartAiming();

            throwForceTimer = Math.Min(throwForceTimer + Runner.DeltaTime, data.SecondsBeforeMaxThrowForce);
            
            var forcePercent = ThrowForcePercent;
            RumbleSystem.Instance.SetRumble(throwRumbleKey, lowFrequencyThrowRumbleCurve.Evaluate(forcePercent), highFrequencyThrowRumbleCurve.Evaluate(forcePercent));
        }

        private bool CanThrow()
        {
            if (!inventory.HasHomework)
                return false;
            
            if (IsDashing)
                return false;
            
            return true;
        }

        private void StartAiming()
        {
            throwForceTimer = 0f;
            IsAiming = true;
            IsThrowing = false;
        }

        private void CancelAiming()
        {
            RumbleSystem.Instance.StopRumble(throwRumbleKey);
            IsThrowing = false;
            IsAiming = false;
        }

        private void Throw()
        {
            RumbleSystem.Instance.StopRumble(throwRumbleKey);

            IsAiming = false;
            IsThrowing = true;

            var throwForce = Math.Max(data.MinThrowForce, ThrowForcePercent * data.MaxThrowForce);
            inventory.DropEverything(transform.forward + Vector3.up * data.ThrowVerticality, throwForce);
        }
    }
}