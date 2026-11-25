using UnityEngine;

namespace FlameBlame.Firefighter.Simulation
{
    public class TieDrag
    {
        private const float PullSmoothingSeconds = 0.15f;

        private readonly TieDragSettings _settings;
        private Vector3 _pull;

        public TieDrag(TieDragSettings settings)
        {
            _settings = settings;
        }

        public Vector3 Step(Vector3 tension, bool isPulled, Vector3 velocity, float mass, float deltaTime)
        {
            _pull = Vector3.Lerp(_pull, tension, deltaTime / PullSmoothingSeconds);

            var horizontalVelocity = new Vector3(velocity.x, 0f, velocity.z);
            if (!isPulled)
                return Vector3.ClampMagnitude(-horizontalVelocity * (mass / _settings.BrakeSeconds), _settings.MaxForce);

            var horizontalPull = new Vector3(_pull.x, 0f, _pull.z);
            var excess = Mathf.Max(0f, horizontalPull.magnitude - _settings.MinTension);
            var direction = horizontalPull.normalized;
            if (Vector3.Dot(horizontalVelocity, direction) >= _settings.MaxSpeed)
                return Vector3.zero;

            return direction * Mathf.Min(excess * _settings.ForceMultiplier, _settings.MaxForce);
        }
    }
}
