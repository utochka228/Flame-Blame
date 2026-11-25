using System.Collections.Generic;
using UnityEngine;

namespace FlameBlame.Hose.Components
{
    public class HosePinchSensor : MonoBehaviour
    {
        private const float SqueezeRadiusFraction = 0.8f;

        [SerializeField] private HoseSegment _hose;

        private bool[] _pinched;
        private float _secondsSinceCheck;

        public IReadOnlyList<bool> Pinched => _pinched;

        private void Awake()
        {
            _pinched = new bool[_hose.Config.ParticleCount];
        }

        private void FixedUpdate()
        {
            _secondsSinceCheck += Time.fixedDeltaTime;
            if (_secondsSinceCheck < 1f / _hose.Config.PinchChecksPerSecond) return;

            _secondsSinceCheck = 0f;
            Check();
        }

        private void Check()
        {
            var positions = _hose.Positions;
            var config = _hose.Config;
            for (var index = 0; index < _pinched.Length; index++)
                _pinched[index] = Physics.CheckSphere(positions[index], config.ParticleRadius * SqueezeRadiusFraction, config.PinchLayers,
                    QueryTriggerInteraction.Ignore);
        }
    }
}
