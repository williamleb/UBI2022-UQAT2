using System;
using Fusion;
using UnityEngine;

namespace VFX
{
    public class DustTrailController : NetworkBehaviour
    {
        [SerializeField] private AnimationCurve interpolation;
        [SerializeField] private Transform targetToFollow;

        [SerializeField] private ParticleSystem dustRound;
        [SerializeField] private ParticleSystem dustEdgy;

        [SerializeField] private ParticleSystemValues baseValues;

        //[SerializeField] private ParticleSystemValues midValues;
        [SerializeField] private ParticleSystemValues highValues;

        private Vector3 offset;

        private void Awake() => offset = transform.localPosition;

        private void LateUpdate() => transform.position = targetToFollow.position - targetToFollow.forward + offset;

        public void UpdateDustTrail(float interpolationValue)
        {
            if (interpolationValue > 0)
            {
                if (dustEdgy.isStopped)
                {
                    dustEdgy.Play();
                    dustRound.Play();
                }
            }
            else
            {
                dustEdgy.Stop();
                dustRound.Stop();
            }

            float value = interpolation.Evaluate(Mathf.Clamp01(interpolationValue));
            UpdateParticleSystem(dustEdgy, value, true, -targetToFollow.forward);
            UpdateParticleSystem(dustRound, value, false, -targetToFollow.forward);
        }

        private void UpdateParticleSystem(ParticleSystem ps, float lerpValue, bool isEdgy, Vector3 direction)
        {
            var mainModule = ps.main;
            mainModule.startLifetime = Mathf.Lerp(baseValues.Lifetime, highValues.Lifetime, lerpValue);
            mainModule.startSpeed = Mathf.Lerp(baseValues.StartSpeed, highValues.StartSpeed, lerpValue);
            mainModule.startSize = Mathf.Lerp(baseValues.StartSize, highValues.StartSize, lerpValue);
            mainModule.simulationSpeed = Mathf.Lerp(baseValues.SimulationSpeed, highValues.SimulationSpeed, lerpValue);
            var emissionModule = ps.emission;
            if (isEdgy)
                emissionModule.rateOverDistance = Mathf.Lerp(baseValues.EmissionRateOverDistanceEdgy,
                    highValues.EmissionRateOverDistanceEdgy, lerpValue);
            else
                emissionModule.rateOverDistance = Mathf.Lerp(baseValues.EmissionRateOverDistanceRound,
                    highValues.EmissionRateOverDistanceRound, lerpValue);

            var shapeModule = ps.shape;
            shapeModule.angle = Mathf.Lerp(baseValues.Angle, highValues.Angle, lerpValue);

            var lvolModule = ps.limitVelocityOverLifetime;
            lvolModule.dampen = Mathf.Lerp(baseValues.Dampen, highValues.Dampen, lerpValue);

            var folModule = ps.forceOverLifetime;
            Vector3 baseMin = direction * baseValues.ForceLifetimeConstantXMin;
            Vector3 baseMax = direction * baseValues.ForceLifetimeConstantXMax;
            Vector3 highMin = direction * highValues.ForceLifetimeConstantXMin;
            Vector3 highMax = direction * highValues.ForceLifetimeConstantXMax;
            folModule.x = new ParticleSystem.MinMaxCurve(
                Mathf.Lerp(baseMin.x, highMin.x, lerpValue),
                Mathf.Lerp(baseMax.x, highMax.x, lerpValue));
            folModule.z = new ParticleSystem.MinMaxCurve(
                Mathf.Lerp(baseMin.z, highMin.z, lerpValue),
                Mathf.Lerp(baseMax.z, highMax.z, lerpValue));
        }
    }

    [Serializable]
    public struct ParticleSystemValues
    {
        public float Lifetime;
        public float StartSpeed;
        public float StartSize;
        public float SimulationSpeed;
        public float EmissionRateOverDistanceEdgy;
        public float EmissionRateOverDistanceRound;
        public float Angle;
        public float Dampen;
        public float ForceLifetimeConstantXMin;
        public float ForceLifetimeConstantXMax;
    }
}