using UnityEngine;

namespace FlameBlame.Firefighter.Simulation
{
    public class LookState
    {
        private float _degreesPerPixel;
        private float _pitchLimitDegrees;
        private float _pitchSign;

        public float Yaw { get; private set; }
        public float Pitch { get; private set; }

        public LookState(float degreesPerPixel, float pitchLimitDegrees, float initialYaw, bool isInvertedY = false)
        {
            Configure(degreesPerPixel, pitchLimitDegrees, isInvertedY);
            Yaw = initialYaw;
        }

        public void Configure(float degreesPerPixel, float pitchLimitDegrees, bool isInvertedY)
        {
            _degreesPerPixel = degreesPerPixel;
            _pitchLimitDegrees = pitchLimitDegrees;
            _pitchSign = isInvertedY ? 1f : -1f;
        }

        public void Apply(Vector2 lookDelta)
        {
            Yaw += lookDelta.x * _degreesPerPixel;
            Pitch = Mathf.Clamp(Pitch + _pitchSign * lookDelta.y * _degreesPerPixel, -_pitchLimitDegrees, _pitchLimitDegrees);
        }
    }
}
