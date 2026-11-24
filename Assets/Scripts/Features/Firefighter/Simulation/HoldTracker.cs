using UnityEngine;

namespace FlameBlame.Firefighter.Simulation
{
    public class HoldTracker
    {
        private readonly float _followSeconds;
        private readonly float _maxDistance;

        private Vector3 _velocity;

        public Vector3 Position { get; private set; }
        public Vector3 AimDirection { get; private set; }

        public HoldTracker(float followSeconds, float maxDistance)
        {
            _followSeconds = followSeconds;
            _maxDistance = maxDistance;
        }

        public void Begin(Vector3 particlePosition, Vector3 aimDirection)
        {
            Position = particlePosition;
            AimDirection = aimDirection;
            _velocity = Vector3.zero;
        }

        public void Step(Vector3 holdPoint, Vector3 viewDirection, float deltaTime)
        {
            var position = Position;
            Position = Vector3.SmoothDamp(position, holdPoint, ref _velocity, _followSeconds, Mathf.Infinity, deltaTime);
            AimDirection = Vector3.Slerp(AimDirection, viewDirection, 1f - Mathf.Exp(-deltaTime / _followSeconds));
        }

        public bool IsTooFar(Vector3 particlePosition, Vector3 holderPosition)
        {
            return Vector3.Distance(particlePosition, holderPosition) > _maxDistance;
        }
    }
}
