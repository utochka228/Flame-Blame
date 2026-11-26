using UnityEngine;

namespace FlameBlame.Firefighter.Simulation
{
    public class CameraSpring
    {
        private const float MaxSubstepSeconds = 1f / 120f;
        private const float RestOffset = 1e-4f;
        private const float RestVelocity = 1e-3f;

        private float _velocity;

        public float Offset { get; private set; }

        public void Kick(float velocity)
        {
            _velocity += velocity;
        }

        public void Step(float deltaTime, float angularFrequency, float dampingRatio)
        {
            while (deltaTime > 0f)
            {
                var substep = Mathf.Min(deltaTime, MaxSubstepSeconds);
                deltaTime -= substep;
                var acceleration = -angularFrequency * angularFrequency * Offset - 2f * dampingRatio * angularFrequency * _velocity;
                _velocity += acceleration * substep;
                Offset += _velocity * substep;
            }

            if (Mathf.Abs(Offset) < RestOffset && Mathf.Abs(_velocity) < RestVelocity)
            {
                Offset = 0f;
                _velocity = 0f;
            }
        }
    }
}
