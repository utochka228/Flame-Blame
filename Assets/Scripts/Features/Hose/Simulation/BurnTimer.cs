using UnityEngine;

namespace FlameBlame.Hose.Simulation
{
    public class BurnTimer
    {
        private readonly float _seconds;

        public BurnTimer(float seconds)
        {
            _seconds = seconds;
        }

        public float Progress { get; private set; }

        public bool Step(bool isHeated, float deltaTime)
        {
            var direction = isHeated ? 1f : -1f;
            Progress = Mathf.Clamp01(Progress + direction * deltaTime / _seconds);
            if (Progress < 1f) return false;

            Progress = 0f;
            return true;
        }
    }
}
