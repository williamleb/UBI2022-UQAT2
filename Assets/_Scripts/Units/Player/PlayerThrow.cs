using System;
using Fusion;
using Systems;
using Systems.Network;
using UnityEngine;

namespace Units.Player
{
    public partial class PlayerEntity
    {
        [Networked] private NetworkBool IsAiming { get; set; } = false; 

        private float throwForceTimer = 0f;

        private float ThrowForcePercent => data.SecondsBeforeMaxThrowForce != 0 ? throwForceTimer / data.SecondsBeforeMaxThrowForce : 1f;

        private void ThrowUpdate(NetworkInputData inputData)
        {
            if (!CanThrow())
            {
                if (IsAiming)
                    CancelAiming();
                
                return;
            }

            if (inputData.IsThrow)
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
            if (!IsAiming)
            {
                IsAiming = true;
                StartAiming();
            }

            throwForceTimer = Math.Min(throwForceTimer + Runner.DeltaTime, data.SecondsBeforeMaxThrowForce);
            
            // TODO Rumble controller with more force (with ThrowForcePercent)

            var forcePercent = ThrowForcePercent;
            RumbleSystem.Instance.SetRumble(this, forcePercent, Math.Abs(forcePercent - 1f) < 0.1f ? 0.75f : 0.25f);
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

            // TODO Start aiming animation
        }

        private void CancelAiming()
        {
            RumbleSystem.Instance.StopRumble(this);

            // TODO Stop aiming animation
        }

        private void Throw()
        {
            RumbleSystem.Instance.StopRumble(this);

            // TODO Stop aiming animation
            // TODO Throw animation
            
            IsAiming = false;

            var throwForce = Math.Max(data.MinThrowForce, ThrowForcePercent * data.MaxThrowForce);
            inventory.DropEverything(transform.forward + Vector3.up * data.ThrowVerticality, throwForce);
        }
    }
}