using System;
using Fusion;
using Systems;
using Systems.Network;
using Systems.Sound;
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

        [Networked] private float ThrowForcePercent { get; set; }

        private void InitThrow()
        {
            throwRumbleKey = RumbleSystem.Instance.GenerateNewRumbleKeyFromBehaviour(this);
            SoundSystem.Instance.InitAimCharge(this);
        }

        private void ThrowUpdateOnAllClients()
        {
            if (!IsThrowing)
                return;
            
            SetAimChargePercentValueLocally(ThrowForcePercent);
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
            
            ThrowForcePercent = data.SecondsBeforeMaxThrowForce != 0 ? throwForceTimer / data.SecondsBeforeMaxThrowForce : 1f;
            RumbleSystem.Instance.SetRumble(throwRumbleKey, lowFrequencyThrowRumbleCurve.Evaluate(ThrowForcePercent), highFrequencyThrowRumbleCurve.Evaluate(ThrowForcePercent));
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
            SoundSystem.Instance.InitAimCharge(this);
            PlayAimHoldSound();

            throwForceTimer = 0f;
            IsAiming = true;
            IsThrowing = false;
            ThrowForcePercent = 0f;
        }

        private void CancelAiming()
        {
            StopAimHoldSound();
            RumbleSystem.Instance.StopRumble(throwRumbleKey);
            
            IsThrowing = false;
            IsAiming = false;
            ThrowForcePercent = 0f;
        }

        private void Throw()
        {
            PlayAimReleaseSound();
            RumbleSystem.Instance.StopRumble(throwRumbleKey);

            IsAiming = false;
            IsThrowing = true;

            var throwForce = Math.Max(data.MinThrowForce, ThrowForcePercent * data.MaxThrowForce);
            inventory.DropEverything(transform.forward + Vector3.up * data.ThrowVerticality, throwForce);
            ThrowForcePercent = 0f;
        }
    }
}