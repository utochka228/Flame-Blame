using UnityEngine;

namespace FlameBlame.Firefighter.Simulation
{
    public class LookState
    {
        private float _degreesPerPixel;
        private float _pitchLimitDegrees;

        public float Yaw { get; private set; }
        public float Pitch { get; private set; }

        public LookState(float degreesPerPixel, float pitchLimitDegrees, float initialYaw)
        {
            Configure(degreesPerPixel, pitchLimitDegrees);
            Yaw = initialYaw;
        }

        public void Configure(float degreesPerPixel, float pitchLimitDegrees)
        {
            _degreesPerPixel = degreesPerPixel;
            _pitchLimitDegrees = pitchLimitDegrees;
        }

        public void Apply(Vector2 lookDelta)
        {
            Yaw += lookDelta.x * _degreesPerPixel;
            Pitch = Mathf.Clamp(Pitch - lookDelta.y * _degreesPerPixel, -_pitchLimitDegrees, _pitchLimitDegrees);
        }
    }
}
