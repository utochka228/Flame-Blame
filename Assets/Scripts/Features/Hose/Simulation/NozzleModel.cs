using UnityEngine;

namespace FlameBlame.Hose.Simulation
{
    public class NozzleModel
    {
        private readonly float _maxJetLength;
        private readonly float _baseSpreadDegrees;
        private readonly float _thumbSpreadDegrees;
        private readonly float _recoilAcceleration;

        public NozzleModel(float maxJetLength, float baseSpreadDegrees, float thumbSpreadDegrees, float recoilAcceleration)
        {
            _maxJetLength = maxJetLength;
            _baseSpreadDegrees = baseSpreadDegrees;
            _thumbSpreadDegrees = thumbSpreadDegrees;
            _recoilAcceleration = recoilAcceleration;
        }

        public NozzleOutput Evaluate(float tipPressureFraction, float thumbFraction)
        {
            var flow = Mathf.Clamp01(tipPressureFraction);
            return new NozzleOutput(
                flow,
                _maxJetLength * flow,
                Mathf.Lerp(_baseSpreadDegrees, _thumbSpreadDegrees, Mathf.Clamp01(thumbFraction)),
                _recoilAcceleration * Mathf.Max(0f, tipPressureFraction));
        }
    }
}
