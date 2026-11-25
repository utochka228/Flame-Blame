using FlameBlame.Hose.Components;
using FlameBlame.Hose.Simulation;
using FlameBlame.Water.Components;
using UnityEngine;

namespace FlameBlame.Hose.Presentation
{
    public class HoseLeakSpray : MonoBehaviour
    {
        private const float MinSprayPressureFraction = 0.02f;
        private const float MinBurstLoss = 1e-3f;

        [SerializeField] private HoseSegment _hose;
        [SerializeField] private HoseBurstMonitor _burstMonitor;
        [SerializeField] private ParticleSystem _particles;
        [SerializeField] private PuddleField _puddles;
        [SerializeField] [Min(0)] private int _popBurstCount = 30;

        private float _fullSprayRate;

        private void Awake()
        {
            _fullSprayRate = _particles.emission.rateOverTime.constant;
        }

        private void OnEnable()
        {
            _burstMonitor.Popped += OnPopped;
        }

        private void OnDisable()
        {
            _burstMonitor.Popped -= OnPopped;
        }

        private void LateUpdate()
        {
            if (!_burstMonitor.IsLeaking) return;

            var breach = LargestBreach();
            var leakIndex = breach.Index;
            var strength = StrengthOf(breach);
            var pressureFraction = UpstreamPressureFraction(breach);
            _particles.transform.position = _hose.Positions[leakIndex];
            var hasFlow = pressureFraction > MinSprayPressureFraction;
            if (hasFlow)
                AddPuddleWater(leakIndex, pressureFraction * strength);

            UpdateSpray(hasFlow, strength);
        }

        private float UpstreamPressureFraction(Breach breach)
        {
            var pressures = _hose.PressureFractions;
            var before = pressures[Mathf.Max(breach.FirstIndex - 1, 0)];
            var after = pressures[Mathf.Min(breach.LastIndex + 1, pressures.Count - 1)];
            return Mathf.Max(before, after);
        }

        private float StrengthOf(Breach breach) => Mathf.Clamp01(breach.Loss / Mathf.Max(_hose.Config.BurstLeakLoss, MinBurstLoss));

        private Breach LargestBreach()
        {
            var breaches = _burstMonitor.Breaches;
            var largest = breaches[0];
            for (var index = 1; index < breaches.Count; index++)
            {
                if (breaches[index].Loss > largest.Loss)
                    largest = breaches[index];
            }

            return largest;
        }

        private void AddPuddleWater(int leakIndex, float pressureFraction)
        {
            if (!_puddles) return;

            var volume = _hose.Config.LeakPuddleVolumePerSecond * pressureFraction * Time.deltaTime;
            _puddles.AddWater(_hose.Positions[leakIndex], volume);
        }

        private void UpdateSpray(bool hasFlow, float strength)
        {
            var emission = _particles.emission;
            emission.rateOverTime = _fullSprayRate * strength;
            if (hasFlow && !_particles.isEmitting)
                _particles.Play();
            else if (!hasFlow && _particles.isEmitting)
                _particles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        private void OnPopped()
        {
            _particles.transform.position = _hose.Positions[_burstMonitor.WeakPointIndex];
            _particles.Emit(_popBurstCount);
        }
    }
}
