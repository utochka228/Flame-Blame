using FlameBlame.Hose.Components;
using UnityEngine;

namespace FlameBlame.Hose.Presentation
{
    public class HoseLeakSpray : MonoBehaviour
    {
        [SerializeField] private HoseSegment _hose;
        [SerializeField] private HoseBurstMonitor _burstMonitor;
        [SerializeField] private ParticleSystem _particles;

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
            if (_burstMonitor.HasBurst)
                _particles.transform.position = _hose.Positions[_burstMonitor.WeakPointIndex];
        }

        private void OnPopped()
        {
            _particles.Play();
        }
    }
}
