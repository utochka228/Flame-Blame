using FlameBlame.Hose.Components;
using UnityEngine;

namespace FlameBlame.Hose.Presentation
{
    public class HoseLeakSpray : MonoBehaviour
    {
        private const float MinSprayPressureFraction = 0.02f;

        [SerializeField] private HoseSegment _hose;
        [SerializeField] private HoseBurstMonitor _burstMonitor;
        [SerializeField] private ParticleSystem _particles;
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
            _particles.transform.position = _hose.Positions[leakIndex];
            var hasFlow = _hose.PressureFractions[leakIndex] > MinSprayPressureFraction;
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
