using System;
using UnityEngine;

namespace VFX
{
    public class DustTrailController : MonoBehaviour
    {
        [SerializeField] private AnimationCurve interpolation;

        [SerializeField] private ParticleSystem dustRound;
        [SerializeField] private ParticleSystem dustEdgy;
        [SerializeField] private ParticleSystemValues baseValues;
        //[SerializeField] private ParticleSystemValues midValues;
        [SerializeField] private ParticleSystemValues highValues;

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
            UpdateParticleSystem(dustEdgy, value, true);
            UpdateParticleSystem(dustRound, value, false);
        }

        private void UpdateParticleSystem(ParticleSystem ps, float lerpValue, bool isEdgy)
        {
            var mainModule = ps.main;
            mainModule.startLifetime = Mathf.Lerp(baseValues.Lifetime, highValues.Lifetime, lerpValue);
            mainModule.startSpeed = Mathf.Lerp(baseValues.StartSpeed, highValues.StartSpeed, lerpValue);
            mainModule.startSize = Mathf.Lerp(baseValues.StartSize, highValues.StartSize, lerpValue);
            mainModule.simulationSpeed = Mathf.Lerp(baseValues.SimulationSpeed, highValues.SimulationSpeed, lerpValue);
            var emissionModule = ps.emission;
            if (isEdgy)
                emissionModule.rateOverDistance = Mathf.Lerp(baseValues.EmissionRateOverDistanceEdgy, highValues.EmissionRateOverDistanceEdgy, lerpValue);
            else
                emissionModule.rateOverDistance = Mathf.Lerp(baseValues.EmissionRateOverDistanceRound, highValues.EmissionRateOverDistanceRound, lerpValue);
            
            var shapeModule = ps.shape;
            shapeModule.angle = Mathf.Lerp(baseValues.Angle, highValues.Angle, lerpValue);
            
            var lvolModule = ps.limitVelocityOverLifetime;
            lvolModule.dampen = Mathf.Lerp(baseValues.Dampen, highValues.Dampen, lerpValue);

            var folModule = ps.forceOverLifetime;
            folModule.x = new ParticleSystem.MinMaxCurve(
                Mathf.Lerp(baseValues.ForceLifetimeConstantXMin,highValues.ForceLifetimeConstantXMin,lerpValue),
                Mathf.Lerp(baseValues.ForceLifetimeConstantXMax,highValues.ForceLifetimeConstantXMax,lerpValue));
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
