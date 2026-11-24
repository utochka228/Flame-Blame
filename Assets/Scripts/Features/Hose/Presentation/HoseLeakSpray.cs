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

        private void LateUpdate()
        {
            if (!_burstMonitor.HasBurst) return;

            _particles.transform.position = _hose.Positions[_burstMonitor.WeakPointIndex];
            var hasFlow = _hose.PressureFractions[_burstMonitor.WeakPointIndex] > MinSprayPressureFraction;
            if (hasFlow && !_particles.isEmitting)
                _particles.Play();
            else if (!hasFlow && _particles.isEmitting)
                _particles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }
}
