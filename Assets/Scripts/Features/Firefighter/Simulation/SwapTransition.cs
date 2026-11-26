using UnityEngine;

namespace FlameBlame.Firefighter.Simulation
{
    public class SwapTransition
    {
        private float _dropSeconds;
        private float _liftSeconds;
        private float _elapsedSeconds;

        public bool IsActive { get; private set; }
        public float VisorFraction { get; private set; }

        public SwapTransition(float dropSeconds, float liftSeconds)
        {
            Configure(dropSeconds, liftSeconds);
        }

        public void Configure(float dropSeconds, float liftSeconds)
        {
            _dropSeconds = dropSeconds;
            _liftSeconds = liftSeconds;
        }

        public void Begin()
        {
            IsActive = true;
            _elapsedSeconds = 0f;
        }

        public bool Step(float deltaTime)
        {
            if (!IsActive) return false;

            var wasDropping = _elapsedSeconds < _dropSeconds;
            _elapsedSeconds += deltaTime;
            if (_elapsedSeconds < _dropSeconds)
            {
                VisorFraction = _elapsedSeconds / _dropSeconds;
                return false;
            }

            var liftFraction = Mathf.Clamp01((_elapsedSeconds - _dropSeconds) / _liftSeconds);
            VisorFraction = 1f - liftFraction;
            IsActive = liftFraction < 1f;
            return wasDropping;
        }
    }
}
