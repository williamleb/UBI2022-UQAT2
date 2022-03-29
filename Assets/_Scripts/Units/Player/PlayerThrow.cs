using System;
using Canvases.Markers;
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
        [SerializeField] private ThrowChargeMarkerReceptor throwMarker;

        private RumbleKey throwRumbleKey;
        
        [Networked(OnChanged = nameof(OnIsAimingChanged))] private NetworkBool IsAiming { get; set; } = false;
        [Networked] private NetworkBool IsThrowing { get; set; } = false;

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
            {
                SetAimSoundChargePercentValueLocally(ThrowForcePercent);
            }

            if (throwMarker)
                throwMarker.ChargeAmount = ThrowForcePercent;
        }
        
        private void ThrowUpdate(NetworkInputData inputData)
        {
            if (!Runner.IsForward) return;

            if (!CanThrow())
            {
                if (IsAiming || IsThrowing || RumbleSystem.Instance.HasRumble)
                    CancelAimingAndThrowing();

                return;
            }

            if (inputData.IsThrow && !InMenu)
            {
                UpdateAim();
            }
            else
            {
                if (IsAiming)
                {
                    UpdateThrowState();
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

            if (isRagdoll)
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

        private void CancelAimingAndThrowing()
        {
            RumbleSystem.Instance.StopRumble(throwRumbleKey);
            StopAimHoldSoundLocally();
            IsAiming = false;
            IsThrowing = false;
            ThrowForcePercent = 0f;
        }

        private void UpdateThrowState()
        {
            RumbleSystem.Instance.StopRumble(throwRumbleKey);

            IsThrowing = true;
            IsAiming = false;
        }

        public void ThrowOnAnimEvent()
        {
            StopAimHoldSoundLocally();
            var throwForce = Math.Max(data.MinThrowForce, ThrowForcePercent * data.MaxThrowForce);
            inventory.DropEverything(transform.forward + Vector3.up * data.ThrowVerticality, throwForce);
            ThrowForcePercent = 0f;
            PlayAimReleaseSoundLocally();
        }

        private void UpdateAimingSound()
        {
            if (IsAiming)
            {
                PlayAimHoldSoundLocally();

                if (throwMarker)
                {
                    throwMarker.ChargeAmount = 0f;
                    throwMarker.Activate();
                }
            }
            else
            {
                if (!IsThrowing)
                    StopAimHoldSoundLocally();
                
                if (throwMarker)
                    throwMarker.Deactivate();
            }
        }

        private static void OnIsAimingChanged(Changed<PlayerEntity> changed)
        {
            changed.Behaviour.UpdateAimingSound();
        }
    }
}