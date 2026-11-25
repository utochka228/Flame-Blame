using FlameBlame.Hose.Components;
using FlameBlame.Water.Components;
using UnityEngine;

namespace FlameBlame.Hose.Presentation
{
    public class HoseLeakSpray : MonoBehaviour
    {
        private const float MinSprayPressureFraction = 0.02f;

        [SerializeField] private HoseSegment _hose;
        [SerializeField] private HoseBurstMonitor _burstMonitor;
        [SerializeField] private ParticleSystem _particles;
        [SerializeField] private PuddleField _puddles;
        [SerializeField] [Min(0)] private int _popBurstCount = 30;

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

            var leakIndex = _burstMonitor.Breaches[0].Index;
            var pressureFraction = _hose.PressureFractions[leakIndex];
            _particles.transform.position = _hose.Positions[leakIndex];
            var hasFlow = pressureFraction > MinSprayPressureFraction;
            if (hasFlow)
                AddPuddleWater(leakIndex, pressureFraction);

            UpdateSpray(hasFlow);
        }

        private void AddPuddleWater(int leakIndex, float pressureFraction)
        {
            if (!_puddles) return;

            var volume = _hose.Config.LeakPuddleVolumePerSecond * pressureFraction * Time.deltaTime;
            _puddles.AddWater(_hose.Positions[leakIndex], volume);
        }

        private void UpdateSpray(bool hasFlow)
        {
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
