using UnityEngine;

namespace FlameBlame.Firefighter.Simulation
{
    public class SwapTimer
    {
        private const float MinIntervalFraction = 0.5f;

        private float _maxSeconds;
        private float _warningSeconds;
        private float _secondsLeft;
        private bool _isRunning;

        public float WarningFraction => _isRunning && _secondsLeft < _warningSeconds ? 1f - _secondsLeft / _warningSeconds : 0f;

        public SwapTimer(float maxSeconds, float warningSeconds)
        {
            Configure(maxSeconds, warningSeconds);
        }

        public void Configure(float maxSeconds, float warningSeconds)
        {
            _maxSeconds = maxSeconds;
            _warningSeconds = warningSeconds;
        }

        public void Restart(float randomFraction)
        {
            _isRunning = _maxSeconds > 0f;
            _secondsLeft = Mathf.Lerp(MinIntervalFraction, 1f, randomFraction) * _maxSeconds;
        }

        public void Stop()
        {
            _isRunning = false;
        }

        public bool Step(float deltaTime, bool canFire)
        {
            if (!_isRunning) return false;

            _secondsLeft = Mathf.Max(0f, _secondsLeft - deltaTime);
            if (_secondsLeft > 0f || !canFire) return false;

            _isRunning = false;
            return true;
        }
    }
}
