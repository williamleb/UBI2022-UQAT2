﻿using System;
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
        
        [Networked(OnChanged = nameof(OnIsAimingChanged))] private NetworkBool IsAiming { get; set; } = false;
        [Networked(OnChanged = nameof(OnIsThrowingChanged))] private NetworkBool IsThrowing { get; set; } = false;

        [Networked] private float ThrowForcePercent { get; set; }
        [Networked] private float ThrowForceTimer { get; set; }

        private void InitThrow()
        {
            throwRumbleKey = RumbleSystem.Instance.GenerateNewRumbleKeyFromBehaviour(this);
        }

        private void ThrowUpdateOnAllClients()
        {
            if (!IsAiming)
                return;

            if (!Object.HasInputAuthority)
                SetAimSoundChargePercentValueLocally(ThrowForcePercent);
        }
        
        private void ThrowUpdate(NetworkInputData inputData)
        {
            if (!Runner.IsForward) return;

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

            ThrowForceTimer = Math.Min(ThrowForceTimer + Runner.DeltaTime, data.SecondsBeforeMaxThrowForce);

            ThrowForcePercent = data.SecondsBeforeMaxThrowForce != 0 ? ThrowForceTimer / data.SecondsBeforeMaxThrowForce : 1f;
            RumbleSystem.Instance.SetRumble(throwRumbleKey, lowFrequencyThrowRumbleCurve.Evaluate(ThrowForcePercent), highFrequencyThrowRumbleCurve.Evaluate(ThrowForcePercent));
            
            if (Object.HasInputAuthority)
                SetAimSoundChargePercentValueLocally(ThrowForcePercent);
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
            ThrowForceTimer = 0f;
            IsAiming = true;
            IsThrowing = false;
            ThrowForcePercent = 0f;
        }

        private void CancelAiming()
        {
            RumbleSystem.Instance.StopRumble(throwRumbleKey);
            
            IsThrowing = false;
            IsAiming = false;
            ThrowForcePercent = 0f;
        }

        private void Throw()
        {
            RumbleSystem.Instance.StopRumble(throwRumbleKey);

            IsAiming = false;
            IsThrowing = true;

            var throwForce = Math.Max(data.MinThrowForce, ThrowForcePercent * data.MaxThrowForce);
            inventory.DropEverything(transform.forward + Vector3.up * data.ThrowVerticality, throwForce);
            ThrowForcePercent = 0f;
        }

        private void UpdateAimingSound()
        {
            if (IsAiming)
            {
                PlayAimHoldSoundLocally();
            }
            else
            {
                StopAimHoldSoundLocally();
            }
        }

        private void UpdateThrowingSound()
        {
            if (IsThrowing)
            {
                PlayAimReleaseSoundLocally();
            }
        }

        private static void OnIsAimingChanged(Changed<PlayerEntity> changed)
        {
            changed.Behaviour.UpdateAimingSound();
        }
        
        private static void OnIsThrowingChanged(Changed<PlayerEntity> changed)
        {
            changed.Behaviour.UpdateThrowingSound();
        }
    }
}