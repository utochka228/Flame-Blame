using UnityEngine;

namespace FlameBlame.Firefighter.Simulation
{
    public class LookState
    {
        private readonly float _degreesPerPixel;
        private readonly float _pitchLimitDegrees;

        public float Yaw { get; private set; }
        public float Pitch { get; private set; }

        public LookState(float degreesPerPixel, float pitchLimitDegrees, float initialYaw)
        {
            _degreesPerPixel = degreesPerPixel;
            _pitchLimitDegrees = pitchLimitDegrees;
            Yaw = initialYaw;
        }

        public void Apply(Vector2 lookDelta)
        {
            Yaw += lookDelta.x * _degreesPerPixel;
            Pitch = Mathf.Clamp(Pitch - lookDelta.y * _degreesPerPixel, -_pitchLimitDegrees, _pitchLimitDegrees);
        }
    }
}
